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

using RyuSocks.Commands.Client;
using System;
using System.Collections.Generic;
using System.Net;

namespace RyuSocks
{
    public class SocksClient : TcpClient
    {
        public readonly Dictionary<byte, Func<SocksClient, IClientCommand>> Commands = new()
        {
            { (byte)ConnectCommand.Id, (client) => new ConnectCommand(client) },
        };

        public SocksClient(IPAddress address, int port) : base(address, port)
        {
        }

        public SocksClient(string address, int port) : base(address, port)
        {
        }

        public SocksClient(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public SocksClient(IPEndPoint endpoint) : base(endpoint)
        {
        }
    }
}
