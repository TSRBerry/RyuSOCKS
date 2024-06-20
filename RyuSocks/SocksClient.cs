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

namespace RyuSocks
{
    public partial class SocksClient
    {
        // TODO: Keep track of the connection state

        private readonly ProxyEndpoint _proxyEndpoint;
        private Socket _socket;
        private bool _serverEndpointReceived;

        protected bool Authenticated;
        protected ClientCommand Command;
        protected IProxyAuth Auth;

        public ProxyCommand RequestCommand = 0;
        public event EventHandler<ProxyEndpoint> OnServerEndpointReceived;
        public IReadOnlyDictionary<AuthMethod, IProxyAuth> OfferedAuthMethods { get; init; } = new Dictionary<AuthMethod, IProxyAuth>();

        public AddressFamily AddressFamily => _proxyEndpoint.ToEndPoint().AddressFamily;
        public SocketType SocketType => Command is { UsesDatagrams: true } ? SocketType.Dgram : SocketType.Stream;
        public ProtocolType ProtocolType => Command is { UsesDatagrams: true } ? ProtocolType.Udp : ProtocolType.Tcp;

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

        public void Authenticate()
        {
            if (Authenticated)
            {
                return;
            }

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

                if (outgoingPacket != null)
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

            // Process command responses until the connection is ready to be used for data or an exception is thrown.
            CommandResponse response = new();
            while (!Command.Ready)
            {
                int receivedBytes = Receive(response.Bytes, SocketFlags.None, out SocketError errorCode);

                if (errorCode != SocketError.Success)
                {
                    throw new SocketException((int)errorCode);
                }

                Debug.Assert(receivedBytes == response.Bytes.Length);
                ProcessCommandResponse(response);
            }

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
            CommandResponse response = new();
            while (!Command.Accepted)
            {
                int receivedBytes = Receive(response.Bytes, SocketFlags.None, out SocketError errorCode);

                if (errorCode != SocketError.Success)
                {
                    throw new SocketException((int)errorCode);
                }

                Debug.Assert(receivedBytes == response.Bytes.Length);
                ProcessCommandResponse(response);
            }
        }

        public void Connect(IPAddress address, int port)
        {
            if (!Authenticated)
            {
                throw new InvalidOperationException($"{nameof(Connect)} can not be invoked before completing authentication with the proxy server.");
            }

            if (RequestCommand == 0)
            {
                throw new InvalidOperationException($"{nameof(Connect)} can not be invoked without requesting a command.");
            }

            // Create the proxy command. This sends the CommandRequest to the server.
            Command = RequestCommand.GetClientCommand()(this, new ProxyEndpoint(new IPEndPoint(address, port)));

            // Process command responses until the connection is ready to be used for data or an exception is thrown.
            CommandResponse response = new();
            while (!Command.Ready)
            {
                int receivedBytes = Receive(response.Bytes, SocketFlags.None, out SocketError errorCode);

                if (errorCode != SocketError.Success)
                {
                    throw new SocketException((int)errorCode);
                }

                Debug.Assert(receivedBytes == response.Bytes.Length);
                ProcessCommandResponse(response);
            }
        }

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

            // Create the proxy command. This sends the CommandRequest to the server.
            Command = RequestCommand.GetClientCommand()(this, new ProxyEndpoint(new DnsEndPoint(host, port)));

            // Process command responses until the connection is ready to be used for data or an exception is thrown.
            CommandResponse response = new();
            while (!Command.Ready)
            {
                int receivedBytes = Receive(response.Bytes, SocketFlags.None, out SocketError errorCode);

                if (errorCode != SocketError.Success)
                {
                    throw new SocketException((int)errorCode);
                }

                Debug.Assert(receivedBytes == response.Bytes.Length);
                ProcessCommandResponse(response);
            }
        }

        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
        {
            int totalWrapperLength = 0;
            int wrapperLength;

            if (Command is { UsesDatagrams: true })
            {
                throw new InvalidOperationException($"{nameof(Send)} can't be used when sending datagrams.");
            }

            if (Command is { Accepted: true })
            {
                buffer = Command.Wrap(buffer, null, out wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            if (Authenticated)
            {
                buffer = Auth.Wrap(buffer, null, out wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            if (Command is { Ready: true, HandlesCommunication: true })
            {
                errorCode = SocketError.Success;
                return Command.Send(buffer) - totalWrapperLength;
            }

            return _socket.Send(buffer, socketFlags, out errorCode) - totalWrapperLength;
        }

        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags) => Send(buffer, socketFlags, out _);
        public int Send(ReadOnlySpan<byte> buffer) => Send(buffer, SocketFlags.None, out _);

        public int Receive(Span<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
        {
            int bytesReceived;
            int wrapperLength;

            if (Command is { UsesDatagrams: true })
            {
                throw new InvalidOperationException($"{nameof(Receive)} can't be used when receiving datagrams.");
            }

            if (Command is { Ready: true, HandlesCommunication: true })
            {
                // TODO: Fix the signature of Send/Receive methods for commands
                bytesReceived = Command.Receive(buffer);
                errorCode = SocketError.Success;
            }
            else
            {
                bytesReceived = _socket.Receive(buffer, socketFlags, out errorCode);
            }

            if (Authenticated)
            {
                // TODO: Make sure this works as expected.
                // TODO: The signature of Unwrap needs to be changed to return void.
                Auth.Unwrap(buffer, out ProxyEndpoint _, out wrapperLength);
                bytesReceived -= wrapperLength;
            }

            if (Command is { Accepted: true })
            {
                Command.Unwrap(buffer, out ProxyEndpoint _, out wrapperLength);
                bytesReceived -= wrapperLength;
            }

            return bytesReceived;
        }

        public int SendTo(ReadOnlySpan<byte> buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            int totalWrapperLength = 0;
            int wrapperLength;

            if (Command is { UsesDatagrams: false })
            {
                throw new InvalidOperationException($"{nameof(SendTo)} can only be used when sending datagrams.");
            }

            ProxyEndpoint remoteEndpoint = remoteEP switch
            {
                IPEndPoint localIPEndPoint => new ProxyEndpoint(localIPEndPoint),
                DnsEndPoint localDnsEndPoint => new ProxyEndpoint(localDnsEndPoint),
                _ => throw new ArgumentException(
                    $"{nameof(EndPoint)} type for argument {nameof(remoteEP)} is not supported. " +
                    $"Only {nameof(IPEndPoint)} and {nameof(DnsEndPoint)} can be used.")
            };

            if (Command is { Accepted: true })
            {
                buffer = Command.Wrap(buffer, remoteEndpoint, out wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            if (Authenticated)
            {
                buffer = Auth.Wrap(buffer, remoteEndpoint, out wrapperLength);
                totalWrapperLength += wrapperLength;
            }

            if (Command is { Ready: true })
            {
                return Command.SendTo(buffer, remoteEP) - totalWrapperLength;
            }

            return _socket.SendTo(buffer, socketFlags, remoteEP) - totalWrapperLength;
        }

        public int SendTo(ReadOnlySpan<byte> buffer, EndPoint remoteEP) => SendTo(buffer, SocketFlags.None, remoteEP);

        public int ReceiveFrom(Span<byte> buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            if (Command is { UsesDatagrams: false })
            {
                throw new InvalidOperationException($"{nameof(ReceiveFrom)} can only be used when receiving datagrams.");
            }

            // TODO: Fix the signature of SendTo/ReceiveTo methods for commands
            int bytesReceived = Command.ReceiveFrom(buffer, ref remoteEP);
            int wrapperLength;

            if (Authenticated)
            {
                // TODO: Make sure this works as expected.
                // TODO: The signature of Unwrap needs to be changed to return void.
                Auth.Unwrap(buffer, out ProxyEndpoint remoteEndpoint, out wrapperLength);
                bytesReceived -= wrapperLength;

                if (remoteEndpoint != null)
                {
                    remoteEP = remoteEndpoint.ToEndPoint();
                }
            }

            if (Command is { Accepted: true })
            {
                Command.Unwrap(buffer, out ProxyEndpoint remoteEndpoint, out wrapperLength);
                bytesReceived -= wrapperLength;

                if (remoteEndpoint != null)
                {
                    remoteEP = remoteEndpoint.ToEndPoint();
                }
            }

            return bytesReceived;
        }

        public int ReceiveFrom(Span<byte> buffer, ref EndPoint remoteEP) => ReceiveFrom(buffer, SocketFlags.None, ref remoteEP);
    }
}
