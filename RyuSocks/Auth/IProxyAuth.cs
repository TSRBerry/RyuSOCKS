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
using System.Security.Authentication;

namespace RyuSocks.Auth
{
    public interface IProxyAuth
    {
        /// <summary>
        /// The amount of additional bytes required for a wrapped packet.
        /// </summary>
        public int WrapperLength { get; }

        /// <summary>
        /// Authenticate the current session using a method-specific sub-negotiation.
        /// </summary>
        /// <param name="incomingPacket">The incoming packet from the server/client. Could be null.</param>
        /// <param name="outgoingPacket">The outgoing packet for the server/client. Could be null.</param>
        /// <returns>Whether the sub-negotiation to authenticate the current session is completed.</returns>
        /// <exception cref="AuthenticationException">Authentication failed.</exception>
        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket);

        /// <summary>
        /// Wrap the packet as required by the negotiated authentication method.
        /// </summary>
        /// <param name="packet">The packet to wrap.</param>
        /// <returns>The wrapped packet.</returns>
        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet);

        /// <summary>
        /// Unwrap the packet and perform the checks as required by the negotiated authentication method.
        /// </summary>
        /// <param name="packet">The packet to unwrap.</param>
        /// <returns>The unwrapped packet.</returns>
        public ReadOnlySpan<byte> Unwrap(ReadOnlySpan<byte> packet);
    }
}
