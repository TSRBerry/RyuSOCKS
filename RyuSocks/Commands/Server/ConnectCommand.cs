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

using RyuSocks.Utils;
using System;
using System.Net;

namespace RyuSocks.Commands.Server
{
    public class ConnectCommand : IServerCommand
    {
        public static ProxyCommand Id => ProxyCommand.Connect;

        private TcpClient _client;
        private readonly SocksSession _parent;

        public ConnectCommand(SocksSession parent)
        {
            _parent = parent;
        }

        public ReplyField Initialize(EndPoint destEndpoint, out EndPoint boundEndpoint)
        {
            boundEndpoint = null;

            _client = destEndpoint switch
            {
                IPEndPoint ipEndpoint => new TcpClient(ipEndpoint),
                DnsEndPoint dnsEndpoint => new TcpClient(dnsEndpoint),
                _ => throw new ArgumentException(
                    $"Endpoint must be of type {typeof(DnsEndPoint)} or {typeof(IPEndPoint)}.", nameof(destEndpoint)),
            };

            if (!_client.Connect())
            {
                ReplyField reply = _client.Error.ToReplyField();
                _client.ResetError();

                return reply;
            }

            boundEndpoint = _client.Socket.LocalEndPoint;

            _client.Received += OnReceived;

            return ReplyField.Succeeded;
        }

        public void SendAsync(Span<byte> buffer)
        {
            if (_client is not { IsConnected: true })
            {
                throw new InvalidOperationException("Client is not connected.");
            }

            _client.SendAsync(buffer);
        }

        public void OnReceived(object sender, ReceivedEventArgs args)
        {
            _parent.SendAsync(args.Buffer, args.Offset, args.Size);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                {
                    _client.Received -= OnReceived;
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
    }
}
