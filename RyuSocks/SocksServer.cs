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
using RyuSocks.Auth;
using RyuSocks.Commands.Server;
using System;
using System.Collections.Generic;
using System.Net;

namespace RyuSocks
{
    public partial class SocksServer : TcpServer
    {
        // TODO: Add (generated) properties for auth methods and commands
        // TODO: Add methods to customize auth method and command behavior
        // TODO: Generate CreateSession() method

        // TODO: Put the constructors in a source generator
        public SocksServer(IPAddress address, int port) : base(address, port)
        {
        }

        public SocksServer(string address, int port) : base(address, port)
        {
        }

        public SocksServer(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public SocksServer(IPEndPoint endpoint) : base(endpoint)
        {
        }
    }
}
