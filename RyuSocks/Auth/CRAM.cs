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
    /// Challenge-Response Authentication Method
    /// (draft-ietf-aft-socks-cram-00)
    /// </summary>
    [AuthMethodImpl(0x05)]
    public class CRAM : IProxyAuth
    {
        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet, ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            throw new NotImplementedException();
        }

        public Span<byte> Unwrap(Span<byte> packet, out ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            throw new NotImplementedException();
        }
    }
}
