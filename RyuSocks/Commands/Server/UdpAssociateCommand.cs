// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RyuSocks.Commands.Server
{
    [ProxyCommandImpl(0x03)]
    public partial class UdpAssociateCommand : ServerCommand, IDisposable
    {
        public override bool HandlesCommunication => true;
        public override bool UsesDatagrams => true;
        // TODO: Improve WrapperLength value.
        //       This is currently set to the maximum length of an EndpointPacket,
        //       but we usually don't need that much space.
        public override int WrapperLength => 262;
        private readonly HashSet<ProxyEndpoint> _destinationEndpoints = [];
        private readonly Socket _socket;
        private SocketAsyncEventArgs _socketReceiveEvent;
        private byte[] _socketReceiveBuffer;
        private IPEndPoint _remoteEndPoint;

        public override int Available { get => _socket.Available; }
        public override bool Blocking { get => _socket.Blocking; set => _socket.Blocking = value; }

        public UdpAssociateCommand(SocksSession session, IPEndPoint boundEndpoint, ProxyEndpoint source) : base(session, boundEndpoint, source)
        {
            if (source == ProxyEndpoint.Null)
            {
                Session.SendAsync(new CommandResponse
                {
                    Version = ProxyConsts.Version,
                    // TODO: Figure out which error should be used here
                    ReplyField = ReplyField.ServerFailure,
                }.AsSpan());

                session.Disconnect();
                return;
            }

            _socket = new Socket(boundEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                _socket.Bind(boundEndpoint);
            }
            catch (SocketException e)
            {
                Session.SendAsync(new CommandResponse
                {
                    Version = ProxyConsts.Version,
                    ReplyField = e.SocketErrorCode.ToReplyField(),
                }.AsSpan());

                session.Disconnect();
                return;
            }

            StartReceiveFrom();

            CommandResponse response = _socket.LocalEndPoint switch
            {
                IPEndPoint ipEndPoint => new CommandResponse(ipEndPoint)
                {
                    Version = ProxyConsts.Version,
                    ReplyField = ReplyField.Succeeded,
                },
                DnsEndPoint dnsEndPoint => new CommandResponse(dnsEndPoint)
                {
                    Version = ProxyConsts.Version,
                    ReplyField = ReplyField.Succeeded,
                },
                _ => throw new InvalidOperationException(
                    $"The type of EndPoint is not supported: {_socket.LocalEndPoint}"),
            };

            Session.SendAsync(response.AsSpan());
        }

        private bool IsClientEndpoint(EndPoint endpoint)
        {
            return endpoint == ProxyEndpoint.ToEndPoint() ||
                   (endpoint is IPEndPoint ipEndpoint && ProxyEndpoint.Contains(ipEndpoint));
        }

        private void StartReceiveFrom()
        {
            _socketReceiveBuffer ??= new byte[_socket.ReceiveBufferSize];
            _socketReceiveEvent?.Dispose();
            _socketReceiveEvent = new SocketAsyncEventArgs();
            _socketReceiveEvent.SetBuffer(_socketReceiveBuffer);
            // TODO: Allow the use of DnsEndPoint
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            _socketReceiveEvent.RemoteEndPoint = _remoteEndPoint;
            _socketReceiveEvent.Completed += OnReceivedFrom;

            if (!_socket.ReceiveFromAsync(_socketReceiveEvent))
            {
                OnReceivedFrom(this, _socketReceiveEvent);
            }
        }

        private void OnReceivedFrom(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                // TODO: Log error
                Session.Disconnect();
                return;
            }

            Span<byte> bufferSpan = _socketReceiveBuffer.AsSpan(0, e.BytesTransferred);

            if (IsClientEndpoint(e.RemoteEndPoint))
            {
                int bufferLength = bufferSpan.Length;
                int requiredWrapperSpace = Session.GetRequiredWrapperSpace();

                if (requiredWrapperSpace != 0)
                {
                    byte[] wrapperBuffer = new byte[bufferSpan.Length + requiredWrapperSpace];
                    bufferSpan.CopyTo(wrapperBuffer);
                    bufferSpan = wrapperBuffer;
                }

                bufferLength = Session.Unwrap(bufferSpan, bufferLength, out ProxyEndpoint remoteEndpoint);

                if (!_destinationEndpoints.Contains(remoteEndpoint) && !Session.IsDestinationValid(remoteEndpoint))
                {
                    return;
                }

                _destinationEndpoints.Add(remoteEndpoint);

                _socket.SendToAsync(bufferSpan[..bufferLength].ToArray(), remoteEndpoint.ToEndPoint());

                return;
            }

            ProxyEndpoint proxyEndpoint = e.RemoteEndPoint switch
            {
                IPEndPoint ipEndpoint => new ProxyEndpoint(ipEndpoint),
                DnsEndPoint dnsEndpoint => new ProxyEndpoint(dnsEndpoint),
                _ => throw new ArgumentException($"The type {e.RemoteEndPoint} is not supported.", nameof(e)),
            };

            if (!_destinationEndpoints.Contains(proxyEndpoint))
            {
                return;
            }

            Session.SendTo(bufferSpan, proxyEndpoint);
        }

        public override int Wrap(Span<byte> buffer, int packetLength, ProxyEndpoint remoteEndpoint)
        {
            UdpPacket packet = new(remoteEndpoint, packetLength);
            buffer[..packetLength].CopyTo(packet.UserData);
            packet.Validate();

            buffer.Clear();
            packet.AsSpan().CopyTo(buffer);

            return packet.Bytes.Length;
        }

        public override int Unwrap(Span<byte> buffer, int packetLength, out ProxyEndpoint remoteEndpoint)
        {
            UdpPacket packet = new(buffer[..packetLength].ToArray());
            remoteEndpoint = packet.ProxyEndpoint;
            packet.Validate();

            buffer.Clear();
            packet.UserData.CopyTo(buffer);

            return packet.UserData.Length;
        }

        public override void Shutdown(SocketShutdown how)
        {
            _socket.Shutdown(how);
        }

        public override void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _socket.GetSocketOption(optionLevel, optionName, optionValue);
        }

        public override object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return _socket.GetSocketOption(optionLevel, optionName);
        }

        public override void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public override void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public override bool Poll(int microSeconds, SelectMode mode)
        {
            return _socket.Poll(microSeconds, mode);
        }

        public override int SendTo(ReadOnlySpan<byte> buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return _socket.SendTo(buffer, socketFlags, remoteEP);
        }

        public override void OnReceived(ReadOnlySpan<byte> buffer)
        {
            throw new InvalidOperationException("This connection can't be used to send data.");
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _socket?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
