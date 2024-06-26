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

using RyuSocks.Auth.Packets;
using RyuSocks.Types;
using System;
using System.Collections.Generic;
using System.Security.Authentication;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Username/Password Authentication
    /// (RFC1929)
    /// </summary>
    [AuthMethodImpl(0x02)]
    public class UsernameAndPassword : IProxyAuth
    {
        public string Username;
        public string Password;
        public bool IsClient = true;

        public Dictionary<string, string> Database { get; set; }

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            if (IsClient)
            {
                if (incomingPacket == null)
                {
                    outgoingPacket = new UsernameAndPasswordRequest(Username, Password).AsSpan();
                    return false;
                }

                UsernameAndPasswordResponse incomingResponsePacket = new(incomingPacket.ToArray());
                incomingResponsePacket.Validate();

                if (incomingResponsePacket.Status == 0)
                {
                    outgoingPacket = null;
                    return true;
                }

                throw new AuthenticationException($"The provided credentials are invalid. {incomingResponsePacket.Status}");
            }

            UsernameAndPasswordRequest requestPacket = new(incomingPacket.ToArray());
            requestPacket.Validate();

            if (Database.TryGetValue(requestPacket.Username, out string password) &&
                password == requestPacket.Password)
            {
                UsernameAndPasswordResponse successResponsePacket = new()
                {
                    Version = AuthConsts.UsernameAndPasswordVersion,
                    Status = 0,
                };

                outgoingPacket = successResponsePacket.AsSpan();

                return true;
            }

            UsernameAndPasswordResponse failureResponsePacket = new()
            {
                Version = AuthConsts.UsernameAndPasswordVersion,
                Status = 1,
            };

            outgoingPacket = failureResponsePacket.AsSpan();

            throw new AuthenticationException("The provided credentials are invalid.");
        }

        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet, ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            wrapperLength = 0;
            return packet;
        }

        public Span<byte> Unwrap(Span<byte> packet, out ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            remoteEndpoint = null;
            wrapperLength = 0;
            return packet;
        }
    }
}
