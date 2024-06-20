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

using RyuSocks.Types;
using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// No authentication required
    /// (RFC1928)
    /// </summary>
    [AuthMethodImpl(0x00)]
    public class NoAuth : IProxyAuth
    {
        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            // Nothing to do here.
            outgoingPacket = null;
            return true;
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
