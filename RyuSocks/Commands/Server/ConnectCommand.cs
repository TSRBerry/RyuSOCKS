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

using RyuSocks.Packets;
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace RyuSocks.Commands.Server
{
    [ProxyCommandImpl(0x01)]
    public partial class ConnectCommand : ServerCommand, IDisposable
    {
        private static readonly IPEndPoint _nullEndPoint = new(0, 0);

        private TcpClient _client;

        public ConnectCommand(SocksSession session, EndPoint destination) : base(session, destination)
        {
            _client = destination switch
            {
                IPEndPoint ipEndpoint => new TcpClient(this, ipEndpoint),
                DnsEndPoint dnsEndpoint => new TcpClient(this, dnsEndpoint),
                _ => throw new ArgumentException(
                    "Invalid EndPoint type provided.", nameof(destination)),
            };

            if (!_client.Connect())
            {
                Session.SendAsync(new CommandResponse(_nullEndPoint)
                {
                    Version = ProxyConsts.Version,
                    ReplyField = _client.Error.ToReplyField(),
                }.Bytes);

                _client.ResetError();
                session.Disconnect();

                return;
            }

            CommandResponse response = _client.Socket.LocalEndPoint switch
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
                    $"The type of LocalEndPoint is not supported: {_client.Socket.LocalEndPoint}"),
            };

            Session.SendAsync(response.Bytes);
        }

        public override void OnReceived(ReadOnlySpan<byte> buffer)
        {
            if (_client is not { IsConnected: true })
            {
                throw new InvalidOperationException("Client is not connected.");
            }

            _client.SendAsync(buffer);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                {
                    _client.Disconnect();
                    _client.Dispose();
                    _client = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        class TcpClient : NetCoreServer.TcpClient
        {
            private readonly ConnectCommand _command;

            public SocketError Error = SocketError.Success;

            public TcpClient(ConnectCommand command, DnsEndPoint endpoint) : base(endpoint)
            {
                _command = command;
            }

            public TcpClient(ConnectCommand command, IPEndPoint endpoint) : base(endpoint)
            {
                _command = command;
            }

            public void ResetError()
            {
                Error = SocketError.Success;
            }

            public override bool Connect()
            {
                ResetError();
                bool result = base.Connect();

                if (!result && Error == SocketError.Success)
                {
                    Error = SocketError.ConnectionRefused;
                }

                return result;
            }

            protected override void OnError(SocketError error)
            {
                Error = error;
            }

            protected override void OnConnected()
            {
                ReceiveAsync();
            }

            protected override void OnReceived(byte[] buffer, long offset, long size)
            {
                _command.Session.SendAsync(buffer.AsSpan((int)offset, (int)size));
            }
        }
    }
}
