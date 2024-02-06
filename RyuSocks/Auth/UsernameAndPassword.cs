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
using RyuSocks.Packets.Auth.UsernameAndPassword;
using System;
using System.Collections.Generic;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Username/Password Authentication
    /// (RFC1929)
    /// </summary>
    [AuthMethodImpl(0x02)]
    public class UsernameAndPassword : IProxyAuth
    {
        private readonly Dictionary<string, string> _database;

        public UsernameAndPassword(Dictionary<string, string> database)
        {
            _database = database;
        }

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            UsernameAndPasswordRequest requestPacket  = new();
            requestPacket.FromArray(incomingPacket.ToArray());
            outgoingPacket = null;

            if (_database.TryGetValue(requestPacket.Username, out string password) && password == requestPacket.Password)
            {
                UsernameAndPasswordResponse successResponsePacket = new UsernameAndPasswordResponse(0x00);
                outgoingPacket = new ReadOnlySpan<byte>(successResponsePacket.ToArray());
                return true;
            }

            // Status 0x05: Connection refused
            UsernameAndPasswordResponse failureResponsePacket = new UsernameAndPasswordResponse(0x05);
            outgoingPacket = new ReadOnlySpan<byte>(failureResponsePacket.ToArray());
            return false;
        }

        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet)
        {
            return packet;
        }

        public ReadOnlySpan<byte> Unwrap(ReadOnlySpan<byte> packet)
        {
            return packet;
        }
    }
}
