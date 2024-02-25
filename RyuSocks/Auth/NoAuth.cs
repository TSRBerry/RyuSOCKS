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
        public int WrapperLength => 0;

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            // Nothing to do here.
            outgoingPacket = null;
            return true;
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
