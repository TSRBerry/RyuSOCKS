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
        public override bool HandlesCommunication => false;
        public override bool UsesDatagrams => false;
        private readonly Socket _clientSocket;
        private SocketAsyncEventArgs _clientReceiveEvent;
        private byte[] _clientReceiveBuffer;

        public ConnectCommand(SocksSession session, IPEndPoint boundEndpoint, ProxyEndpoint destination) : base(session, boundEndpoint, destination)
        {
            _clientSocket = new Socket(destination.ToEndPoint().AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _clientSocket.Connect(destination.ToEndPoint());
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

            ReceiveFromServer();

            CommandResponse response = _clientSocket.LocalEndPoint switch
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
                    $"The type of LocalEndPoint is not supported: {_clientSocket.LocalEndPoint}"),
            };

            Session.SendAsync(response.AsSpan());
        }

        private void ReceiveFromServer()
        {
            if (_clientSocket is not { Connected: true })
            {
                // TODO: Log error
                Session.Disconnect();
                return;
            }

            _clientReceiveBuffer ??= new byte[_clientSocket.ReceiveBufferSize];
            _clientReceiveEvent?.Dispose();
            _clientReceiveEvent = new SocketAsyncEventArgs();
            _clientReceiveEvent.SetBuffer(_clientReceiveBuffer);
            _clientReceiveEvent.Completed += OnClientReceived;

            if (!_clientSocket.ReceiveAsync(_clientReceiveEvent))
            {
                OnClientReceived(this, _clientReceiveEvent);
            }
        }

        private void OnClientReceived(object sender, SocketAsyncEventArgs e)
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
                _clientSocket.Disconnect(false);
                Session.Disconnect();
                return;
            }

            // Copy the buffer with the received data - not sure if that's necessary
            byte[] receivedData = new byte[e.BytesTransferred];
            _clientReceiveBuffer.CopyTo(receivedData.AsSpan());

            Session.SendAsync(receivedData);

            ReceiveFromServer();
        }

        public override void OnReceived(ReadOnlySpan<byte> buffer)
        {
            if (_clientSocket is not { Connected: true })
            {
                throw new InvalidOperationException("Client is not connected.");
            }

            _clientSocket.SendAsync(buffer.ToArray());
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _clientReceiveEvent?.Dispose();
                _clientReceiveEvent = null;
                _clientSocket?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
