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
        public override bool HandlesCommunication => false;
        public override bool UsesDatagrams => false;

        private readonly Socket _serverSocket;
        private Socket _serverSession;
        private SocketAsyncEventArgs _socketAcceptEvent;
        private SocketAsyncEventArgs _sessionReceiveEvent;
        private byte[] _sessionReceiveBuffer;

        public BindCommand(SocksSession session, IPEndPoint boundEndpoint, ProxyEndpoint source) : base(session, boundEndpoint, source)
        {
            _serverSocket = new Socket(boundEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _serverSocket.Bind(boundEndpoint);
            }
            catch (SocketException e)
            {
                Session.SendAsync(new CommandResponse
                {
                    Version = ProxyConsts.Version,
                    ReplyField = e.SocketErrorCode.ToReplyField(),
                }.AsSpan());

                Session.Disconnect();
                return;
            }

            _serverSocket.Listen();
            AcceptNewSession();

            CommandResponse response = _serverSocket.LocalEndPoint switch
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
                _ => throw new InvalidOperationException($"The type of EndPoint is not supported: {_serverSocket.LocalEndPoint}"),
            };

            Session.SendAsync(response.AsSpan());
        }

        private void AcceptNewSession()
        {
            _socketAcceptEvent?.Dispose();
            _socketAcceptEvent = new SocketAsyncEventArgs();
            _socketAcceptEvent.Completed += OnSessionAccepted;

            if (!_serverSocket.AcceptAsync(_socketAcceptEvent))
            {
                OnSessionAccepted(this, _socketAcceptEvent);
            }
        }

        private void ReceiveFromSession()
        {
            if (_serverSession is not { Connected: true })
            {
                // TODO: Log error
                Session.Disconnect();
                return;
            }

            _sessionReceiveBuffer ??= new byte[_serverSession.ReceiveBufferSize];
            _sessionReceiveEvent?.Dispose();
            _sessionReceiveEvent = new SocketAsyncEventArgs();
            _sessionReceiveEvent.SetBuffer(_sessionReceiveBuffer);
            _sessionReceiveEvent.Completed += OnSessionReceived;

            if (!_serverSession.ReceiveAsync(_sessionReceiveEvent))
            {
                OnSessionReceived(this, _sessionReceiveEvent);
            }
        }

        private void OnSessionAccepted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                // TODO: Log error
                return;
            }

            Socket sessionSocket = e.AcceptSocket!;

            if ((!Equals(ProxyEndpoint, ProxyEndpoint.Null) && !ProxyEndpoint.Contains((IPEndPoint)sessionSocket.RemoteEndPoint)) || _serverSession != null)
            {
                sessionSocket.Disconnect(false);
                sessionSocket.Dispose();
                return;
            }

            CommandResponse response = sessionSocket.RemoteEndPoint switch
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
                _ => throw new InvalidOperationException($"The type of RemoteEndPoint is not supported: {sessionSocket.RemoteEndPoint}"),
            };

            Session.SendAsync(response.AsSpan());
            _serverSession = sessionSocket;
            ReceiveFromSession();

            AcceptNewSession();
        }

        private void OnSessionReceived(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                // TODO: Log error
                Session.Disconnect();
                return;
            }

            if (e.BytesTransferred == 0)
            {
                // Connection closed
                _serverSession.Disconnect(false);
                _serverSession.Dispose();
                _serverSession = null;
                return;
            }

            // Copy the buffer with the received data - not sure if that's necessary
            byte[] receivedData = new byte[e.BytesTransferred];
            _sessionReceiveBuffer.CopyTo(receivedData.AsSpan());

            Session.SendAsync(receivedData);

            ReceiveFromSession();
        }

        public override void OnReceived(ReadOnlySpan<byte> buffer)
        {
            if (_serverSession == null)
            {
                throw new InvalidOperationException("No client connected.");
            }

            if (!_serverSession.Connected)
            {
                throw new InvalidOperationException("Client disconnected.");
            }

            _serverSession.SendAsync(buffer.ToArray());
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sessionReceiveEvent?.Dispose();
                _sessionReceiveEvent = null;
                _socketAcceptEvent?.Dispose();
                _socketAcceptEvent = null;
                _serverSession?.Dispose();
                _serverSession = null;
                _serverSocket?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
