/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using RyuSocks.Auth;
using RyuSocks.Commands;
using RyuSocks.Commands.Extensions;
using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RyuSocks
{
    public partial class SocksClient : ISocket, IDisposable
    {
        // TODO: Keep track of the connection state

        private readonly Lock _socketLock = new();
        private readonly ProxyEndpoint _proxyEndpoint;
        private Socket _socket;
        private bool _serverEndpointReceived;

        protected ClientCommand Command;
        protected IProxyAuth Auth;

        public ProxyCommand RequestCommand = 0;
        public event EventHandler<ProxyEndpoint> OnServerEndpointReceived;
        public bool IsCommandAccepted => Command is { Accepted: true };
        public bool Authenticated { get; protected set; }
        public IReadOnlyDictionary<AuthMethod, IProxyAuth> OfferedAuthMethods { get; init; } = new Dictionary<AuthMethod, IProxyAuth>();

        public bool Connected => _socket.Connected;
        public EndPoint LocalEndPoint => _socket.LocalEndPoint;
        public EndPoint ProxiedLocalEndPoint => Command?.ServerEndpoint.ToEndPoint();
        public EndPoint ProxiedRemoteEndPoint => Command?.ClientEndpoints.Count > 0 ? Command.ClientEndpoints[0].ToEndPoint() : null;
        public AddressFamily AddressFamily => _proxyEndpoint.ToEndPoint().AddressFamily;
        public SocketType SocketType => Command is { UsesDatagrams: true } ? SocketType.Dgram : SocketType.Stream;
        public ProtocolType ProtocolType => Command is { UsesDatagrams: true } ? ProtocolType.Udp : ProtocolType.Tcp;

        public int Available
        {
            get
            {
                if (Command is { HandlesCommunication: true })
                {
                    return Command.Available;
                }

                return _socket.Available;
            }
        }

        public bool Blocking
        {
            get
            {
                if (Command is { HandlesCommunication: true })
                {
                    return Command.Blocking;
                }

                return _socket.Blocking;
            }
            set
            {
                if (Command is { HandlesCommunication: true })
                {
                    Command.Blocking = value;
                    return;
                }

                _socket.Blocking = value;
            }
        }

        public bool IsBound
        {
            get
            {
                if (ProxiedLocalEndPoint != null)
                {
                    return true;
                }

                if (RequestCommand != 0)
                {
                    return _socket.IsBound;
                }

                return false;
            }
        }

        public SocksClient(IPEndPoint endpoint)
        {
            _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _proxyEndpoint = new ProxyEndpoint(endpoint);
        }

        public SocksClient(DnsEndPoint endpoint)
        {
            _socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _proxyEndpoint = new ProxyEndpoint(endpoint);
        }

        public SocksClient(IPAddress address, ushort port = ProxyConsts.DefaultPort) : this(new IPEndPoint(address, port)) { }

        protected SocksClient(SocksClient oldSocket)
        {
            _socket = oldSocket._socket;
            _proxyEndpoint = oldSocket._proxyEndpoint;
            _serverEndpointReceived = oldSocket._serverEndpointReceived;
            Authenticated = oldSocket.Authenticated;
            Command = oldSocket.Command;
            Auth = oldSocket.Auth;
            RequestCommand = oldSocket.RequestCommand;
            OfferedAuthMethods = oldSocket.OfferedAuthMethods;
        }

        /// <inheritdoc cref="Socket.Dispose"/>
        public void Dispose()
        {
            if (Command is IDisposable disposableCommand)
            {
                disposableCommand.Dispose();
            }

            _socket.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Socket.Close()"/>
        public void Close()
        {
            _socket.Close();
        }

        /// <inheritdoc cref="Socket.Close(int)"/>
        public void Close(int timeout)
        {
            _socket.Close(timeout);
        }

        public int GetRequiredWrapperSpace()
        {
            int wrapperSpace = 0;

            if (Authenticated)
            {
                wrapperSpace = Auth.WrapperLength;
            }

            if (IsCommandAccepted && wrapperSpace < Command.WrapperLength)
            {
                wrapperSpace = Command.WrapperLength;
            }

            return wrapperSpace;
        }

        public void WaitForCommand(bool accepted)
        {
            if ((accepted && !Command.Accepted) || (!accepted && Command.Ready))
            {
                return;
            }

            CommandResponse response = new();
            int processTry = 0;
            SocketError errorCode = SocketError.Success;
            while (((accepted && !Command.Accepted) || (!accepted && !Command.Ready)) && processTry < 3)
            {
                int receivedBytes = Receive(response.Bytes, SocketFlags.None, out errorCode);

                if (errorCode != SocketError.Success)
                {
                    processTry++;
                    // FIXME: There has to be a better way to solve this.
                    //        This is working around the "resource temporarily not available" errorCode.
                    Thread.Sleep(5 * 1000);
                    continue;
                }

                Debug.Assert(receivedBytes == response.Bytes.Length);
                ProcessCommandResponse(response);
            }

            if (errorCode != SocketError.Success)
            {
                throw new SocketException((int)errorCode);
            }
        }

        public void Authenticate()
        {
            if (Authenticated)
            {
                return;
            }

            lock (_socketLock)
            {
                // Connect to proxy server if we haven't already.
                if (!_socket.Connected)
                {
                    _socket.Connect(_proxyEndpoint.ToEndPoint());
                }

                // Send MethodSelectionRequest.
                var request = new MethodSelectionRequest(OfferedAuthMethods.Keys.ToArray())
                {
                    Version = ProxyConsts.Version,
                };

                int sentBytes = _socket.Send(request.AsSpan());
                Debug.Assert(sentBytes == request.Bytes.Length);

                // Receive MethodSelectionResponse.
                var response = new MethodSelectionResponse();
                int receivedBytes = Receive(response.Bytes, SocketFlags.None, out SocketError errorCode);

                if (errorCode != SocketError.Success)
                {
                    throw new SocketException((int)errorCode);
                }

                Debug.Assert(receivedBytes == response.Bytes.Length);
                response.Validate();

                // Assign authentication method.
                Debug.Assert(OfferedAuthMethods.ContainsKey(response.Method));
                Auth = OfferedAuthMethods[response.Method];

                // Perform method specific authentication.
                byte[] receivedPacket = null;
                while (!Authenticated)
                {
                    Authenticated = Auth.Authenticate(receivedPacket, out ReadOnlySpan<byte> outgoingPacket);

                    if (!outgoingPacket.IsEmpty)
                    {
                        sentBytes = Send(outgoingPacket);
                        Debug.Assert(sentBytes == outgoingPacket.Length);
                    }

                    if (Authenticated)
                    {
                        break;
                    }

                    receivedPacket ??= new byte[_socket.ReceiveBufferSize];
                    Receive(receivedPacket, SocketFlags.None, out errorCode);

                    if (errorCode != SocketError.Success)
                    {
                        throw new SocketException((int)errorCode);
                    }
                }
            }
        }

        protected virtual void ProcessCommandResponse(CommandResponse response)
        {
            response.Validate();

            Command.ProcessResponse(response);

            if (!_serverEndpointReceived && Command.Accepted && Command.ServerEndpoint != null)
            {
                OnServerEndpointReceived?.Invoke(this, Command.ServerEndpoint);
                _serverEndpointReceived = true;
            }
        }

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            if (Command is { HandlesCommunication: true })
            {
                Command.GetSocketOption(optionLevel, optionName, optionValue);
                return;
            }

            _socket.GetSocketOption(optionLevel, optionName, optionValue);
        }

        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            if (Command is { HandlesCommunication: true })
            {
                return Command.GetSocketOption(optionLevel, optionName);
            }

            return _socket.GetSocketOption(optionLevel, optionName);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            // TODO: Should the command get the same socket options as _socket when it gets constructed?

            if (Command is { HandlesCommunication: true })
            {
                Command.SetSocketOption(optionLevel, optionName, optionValue);
                return;
            }

            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            // TODO: Should the command get the same socket options as _socket when it gets constructed?

            if (Command is { HandlesCommunication: true })
            {
                Command.SetSocketOption(optionLevel, optionName, optionValue);
                return;
            }
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public SocksClient Accept()
        {
            if (!Authenticated)
            {
                throw new InvalidOperationException($"{nameof(Accept)} can not be invoked before completing authentication with the proxy server.");
            }

            if (RequestCommand == 0)
            {
                throw new InvalidOperationException($"{nameof(Accept)} can not be invoked without requesting a command.");
            }

            if (Command is not { Accepted: true })
            {
                throw new InvalidOperationException($"{nameof(Accept)} can only be invoked after {nameof(Bind)}.");
            }

            lock (_socketLock)
            {
                // Process command responses until the connection is ready to be used for data or an exception is thrown.
                WaitForCommand(false);

                // Create a new SocksClient for this connection.
                SocksClient session = new(this);

                // Reset the current SocksClient (even though this is not how this is actually supposed to work).
                _socket = new Socket(AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverEndpointReceived = false;
                Authenticated = false;
                Command = null;
                Auth = null;
                RequestCommand = 0;

                return session;
            }
        }

        public void Bind(EndPoint localEP)
        {
            if (!Authenticated)
            {
                throw new InvalidOperationException($"{nameof(Bind)} can not be invoked before completing authentication with the proxy server.");
            }

            if (RequestCommand == 0)
            {
                throw new InvalidOperationException($"{nameof(Bind)} can not be invoked without requesting a command.");
            }

            lock (_socketLock)
            {
                // Get the requested endpoint as a ProxyEndpoint.
                ProxyEndpoint localEndpoint = localEP switch
                {
                    IPEndPoint localIPEndPoint => new ProxyEndpoint(localIPEndPoint),
                    DnsEndPoint localDnsEndPoint => new ProxyEndpoint(localDnsEndPoint),
                    _ => throw new ArgumentException(
                        $"{nameof(EndPoint)} type for argument {nameof(localEP)} is not supported. " +
                        $"Only {nameof(IPEndPoint)} and {nameof(DnsEndPoint)} can be used.")
                };

                // Create the proxy command. This sends the CommandRequest to the server.
                Command = RequestCommand.GetClientCommand()(this, localEndpoint);

                // Process command responses until the command was accepted by the server or an exception is thrown.
                WaitForCommand(true);
            }
        }

        public void Connect(IPEndPoint remoteEP)
        {
            if (!Authenticated)
            {
                throw new InvalidOperationException($"{nameof(Connect)} can not be invoked before completing authentication with the proxy server.");
            }

            if (RequestCommand == 0)
            {
                throw new InvalidOperationException($"{nameof(Connect)} can not be invoked without requesting a command.");
            }

            lock (_socketLock)
            {
                // Create the proxy command. This sends the CommandRequest to the server.
                Command = RequestCommand.GetClientCommand()(this, new ProxyEndpoint(remoteEP));

                // Process command responses until the connection is ready to be used for data or an exception is thrown.
                WaitForCommand(false);
            }
        }

        public void Connect(IPAddress address, int port) => Connect(new IPEndPoint(address, port));

        public void Connect(string host, int port)
        {
            if (!Authenticated)
            {
                throw new InvalidOperationException($"{nameof(Connect)} can not be invoked before completing authentication with the proxy server.");
            }

            if (RequestCommand == 0)
            {
                throw new InvalidOperationException($"{nameof(Connect)} can not be invoked without requesting a command.");
            }

            lock (_socketLock)
            {
                // Create the proxy command. This sends the CommandRequest to the server.
                Command = RequestCommand.GetClientCommand()(this, new ProxyEndpoint(new DnsEndPoint(host, port)));

                // Process command responses until the connection is ready to be used for data or an exception is thrown.
                WaitForCommand(false);
            }
        }

        public void Disconnect()
        {
            lock (_socketLock)
            {
                if (Command is { HandlesCommunication: true })
                {
                    Command.Disconnect();
                }

                // TODO: Set reuseSocket to true once we can handle reconnects here
                _socket.Disconnect(false);
            }
        }

        public void Shutdown(SocketShutdown how)
        {
            lock (_socketLock)
            {
                if (Command is { HandlesCommunication: true })
                {
                    Command.Shutdown(how);
                    return;
                }

                _socket.Shutdown(how);
            }
        }

        public bool Poll(int microSeconds, SelectMode mode)
        {
            lock (_socketLock)
            {
                if (Command is { HandlesCommunication: true })
                {
                    return Command.Poll(microSeconds, mode);
                }

                return _socket.Poll(microSeconds, mode);
            }
        }

        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
        {
            if (Command is { UsesDatagrams: true })
            {
                throw new InvalidOperationException($"{nameof(Send)} can't be used when sending datagrams.");
            }

            lock (_socketLock)
            {
                int bufferLength = buffer.Length;
                int bufferSize = bufferLength + GetRequiredWrapperSpace();

                byte[] sendBufferArray = buffer.ToArray();
                Span<byte> sendBuffer = sendBufferArray;

                if (bufferLength != bufferSize)
                {
                    Array.Resize(ref sendBufferArray, bufferSize);
                    sendBuffer = sendBufferArray;
                }

                if (IsCommandAccepted)
                {
                    bufferLength = Command.Wrap(sendBuffer, bufferLength, null);
                }

                if (Authenticated)
                {
                    bufferLength = Auth.Wrap(sendBuffer, bufferLength, null);
                }

                if (Command is { Ready: true, HandlesCommunication: true })
                {
                    return Command.Send(sendBuffer[..bufferLength], socketFlags, out errorCode);
                }

                return _socket.Send(sendBuffer[..bufferLength], socketFlags, out errorCode);
            }
        }

        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags) => Send(buffer, socketFlags, out _);
        public int Send(ReadOnlySpan<byte> buffer) => Send(buffer, SocketFlags.None, out _);

        public int Receive(Span<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
        {
            if (Command is { UsesDatagrams: true })
            {
                throw new InvalidOperationException($"{nameof(Receive)} can't be used when receiving datagrams.");
            }

            lock (_socketLock)
            {
                int bytesReceived;

                if (Command is { Ready: true, HandlesCommunication: true })
                {
                    bytesReceived = Command.Receive(buffer, socketFlags, out errorCode);
                }
                else
                {
                    bytesReceived = _socket.Receive(buffer, socketFlags, out errorCode);
                }

                if (errorCode != SocketError.Success)
                {
                    return bytesReceived;
                }

                if (Authenticated && bytesReceived > 0)
                {
                    bytesReceived = Auth.Unwrap(buffer, bytesReceived, out ProxyEndpoint _);
                }

                if (IsCommandAccepted && bytesReceived > 0)
                {
                    bytesReceived = Command.Unwrap(buffer, bytesReceived, out ProxyEndpoint _);
                }

                return bytesReceived;
            }
        }

        public int SendTo(ReadOnlySpan<byte> buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            if (Command is { UsesDatagrams: false })
            {
                throw new InvalidOperationException($"{nameof(SendTo)} can only be used when sending datagrams.");
            }

            lock (_socketLock)
            {
                ProxyEndpoint remoteEndpoint = remoteEP switch
                {
                    IPEndPoint localIPEndPoint => new ProxyEndpoint(localIPEndPoint),
                    DnsEndPoint localDnsEndPoint => new ProxyEndpoint(localDnsEndPoint),
                    _ => throw new ArgumentException(
                        $"{nameof(EndPoint)} type for argument {nameof(remoteEP)} is not supported. " +
                        $"Only {nameof(IPEndPoint)} and {nameof(DnsEndPoint)} can be used.")
                };

                int bufferLength = buffer.Length;
                int bufferSize = bufferLength + GetRequiredWrapperSpace();

                byte[] sendBufferArray = buffer.ToArray();
                Span<byte> sendBuffer = sendBufferArray;

                if (bufferLength != bufferSize)
                {
                    Array.Resize(ref sendBufferArray, bufferSize);
                    sendBuffer = sendBufferArray;
                }

                if (IsCommandAccepted)
                {
                    bufferLength = Command.Wrap(sendBuffer, bufferLength, remoteEndpoint);
                }

                if (Authenticated)
                {
                    bufferLength = Auth.Wrap(sendBuffer, bufferLength, remoteEndpoint);
                }

                if (Command is { Ready: true })
                {
                    return Command.SendTo(sendBuffer[..bufferLength], socketFlags, remoteEP);
                }

                return _socket.SendTo(sendBuffer[..bufferLength], socketFlags, remoteEP);
            }
        }

        public int SendTo(ReadOnlySpan<byte> buffer, EndPoint remoteEP) => SendTo(buffer, SocketFlags.None, remoteEP);

        public int ReceiveFrom(Span<byte> buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            if (Command is { UsesDatagrams: false })
            {
                throw new InvalidOperationException($"{nameof(ReceiveFrom)} can only be used when receiving datagrams.");
            }

            lock (_socketLock)
            {
                int bytesReceived = Command.ReceiveFrom(buffer, socketFlags, ref remoteEP);

                if (Authenticated && bytesReceived > 0)
                {
                    bytesReceived = Auth.Unwrap(buffer, bytesReceived, out ProxyEndpoint remoteEndpoint);

                    if (remoteEndpoint != null)
                    {
                        remoteEP = remoteEndpoint.ToEndPoint();
                    }
                }

                if (IsCommandAccepted && bytesReceived > 0)
                {
                    bytesReceived = Command.Unwrap(buffer, bytesReceived, out ProxyEndpoint remoteEndpoint);

                    if (remoteEndpoint != null)
                    {
                        remoteEP = remoteEndpoint.ToEndPoint();
                    }
                }

                return bytesReceived;
            }
        }

        public int ReceiveFrom(Span<byte> buffer, ref EndPoint remoteEP) => ReceiveFrom(buffer, SocketFlags.None, ref remoteEP);
    }
}
