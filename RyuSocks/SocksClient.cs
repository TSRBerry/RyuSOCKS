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

using NetCoreServer;
using System.Net;

namespace RyuSocks
{
    public partial class SocksClient : TcpClient
    {
        // TODO: Add (generated) properties for auth methods and commands
        // TODO: Keep track of the connection state
        // TODO: Expose events and methods to work with this client

        // TODO: Put the constructors in a source generator
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
