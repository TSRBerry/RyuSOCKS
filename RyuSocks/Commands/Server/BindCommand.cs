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
using System.Net;
using System.Net.Sockets;

namespace RyuSocks.Commands.Server
{
    [ProxyCommandImpl(0x02)]
    public partial class BindCommand : ServerCommand, IDisposable
    {
        private TcpServer _server;
        private TcpSession _serverSession;

        public BindCommand(SocksSession session, IPEndPoint boundEndpoint, Destination source) : base(session, boundEndpoint, source)
        {
            _server = new TcpServer(this, boundEndpoint, source);

            _server.Start();
        }

        public override void OnReceived(ReadOnlySpan<byte> buffer)
        {
            if (_serverSession == null)
            {
                throw new InvalidOperationException("No client connected.");
            }

            if (!_serverSession.IsConnected)
            {
                throw new InvalidOperationException("Client disconnected.");
            }

            _serverSession.SendAsync(buffer);
        }

        private void SendErrorReply(SocketError error)
        {
            Session.SendAsync(new CommandResponse
            {
                Version = ProxyConsts.Version,
                ReplyField = error.ToReplyField(),
            }.AsSpan());
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_server != null)
                {
                    _server.Stop();
                    _server.Dispose();
                    _server = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        class TcpServer : NetCoreServer.TcpServer
        {
            private readonly BindCommand _command;
            private readonly Destination _sourceEndpoint;

            public TcpServer(BindCommand command, IPEndPoint endpoint, Destination source) : base(endpoint)
            {
                OptionAcceptorBacklog = 1;
                _command = command;
                _sourceEndpoint = source;
            }

            protected override NetCoreServer.TcpSession CreateSession()
            {
                return new TcpSession(this, _command);
            }

            protected override void OnError(SocketError error)
            {
                _command.SendErrorReply(error);
            }

            protected override void OnConnected(NetCoreServer.TcpSession session)
            {
                if ((!Equals(_sourceEndpoint, Destination.Null) && !_sourceEndpoint.Contains((IPEndPoint)session.Socket.RemoteEndPoint)) || ConnectedSessions > 1)
                {
                    session.Disconnect();
                    return;
                }

                CommandResponse response = session.Socket.RemoteEndPoint switch
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
                        $"The type of RemoteEndPoint is not supported: {session.Socket.RemoteEndPoint}"),
                };

                _command.Session.SendAsync(response.AsSpan());
                _command._serverSession = (TcpSession)session;
            }

            protected override void OnStarted()
            {
                CommandResponse response = Endpoint switch
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
                        $"The type of EndPoint is not supported: {Endpoint}"),
                };

                _command.Session.SendAsync(response.AsSpan());
            }
        }

        class TcpSession : NetCoreServer.TcpSession
        {
            private readonly BindCommand _command;

            public TcpSession(NetCoreServer.TcpServer server, BindCommand command) : base(server)
            {
                _command = command;
            }

            protected override void OnError(SocketError error)
            {
                _command.SendErrorReply(error);
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
