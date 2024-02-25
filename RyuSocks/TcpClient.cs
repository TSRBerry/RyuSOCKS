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
using System.Net.Sockets;

namespace RyuSocks
{
    public class TcpClient : NetCoreServer.TcpClient
    {
        public SocketError Error { get; private set; } = SocketError.Success;
        public event EventHandler<ReceivedEventArgs> Received;

        public TcpClient(IPAddress address, int port) : base(address, port)
        {
        }

        public TcpClient(string address, int port) : base(address, port)
        {
        }

        public TcpClient(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public TcpClient(IPEndPoint endpoint) : base(endpoint)
        {
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

        protected override void OnConnected()
        {
            ReceiveAsync();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Received?.Invoke(this, new ReceivedEventArgs
            {
                Buffer = buffer,
                Offset = offset,
                Size = size,
            });

            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Error = error;
        }
    }
}
