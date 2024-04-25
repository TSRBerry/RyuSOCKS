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
using RyuSocks.Commands;
using System;
using System.Collections.Generic;
using System.Net;

namespace RyuSocks
{
    [SocksClass]
    public partial class SocksServer : TcpServer
    {
        // TODO: Add (generated) properties for auth methods and commands

        public IReadOnlySet<AuthMethod> AcceptableAuthMethods { get; set; } = new HashSet<AuthMethod>();
        public IReadOnlySet<ProxyCommand> OfferedCommands { get; set; } = new HashSet<ProxyCommand>();
        public bool UseAllowList { get; set; }
        public bool UseBlockList { get; set; }
        public IReadOnlyDictionary<IPAddress, ushort[]> AllowedDestinations { get; set; } = new Dictionary<IPAddress, ushort[]>();
        public IReadOnlyDictionary<IPAddress, ushort[]> BlockedDestinations { get; set; } = new Dictionary<IPAddress, ushort[]>();

        #region Multicasting

        public override bool Multicast(ReadOnlySpan<byte> buffer)
        {
            throw new NotSupportedException();
        }

        public override bool Multicast(byte[] buffer)
        {
            throw new NotSupportedException();
        }

        public override bool Multicast(ReadOnlySpan<char> text)
        {
            throw new NotSupportedException();
        }

        public override bool Multicast(byte[] buffer, long offset, long size)
        {
            throw new NotSupportedException();
        }

        public override bool Multicast(string text)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
