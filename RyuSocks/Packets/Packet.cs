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

namespace RyuSocks.Packets
{
    public abstract class Packet
    {
        /// <summary>
        /// The contents of the packet.
        /// </summary>
        public byte[] Bytes { get; protected init; }

        /// <summary>
        /// Validate the structure of the packet.
        /// This method is not supposed to verify the contents of the packet in depth.
        /// </summary>
        public abstract void Validate();

        /// <inheritdoc cref="Bytes"/>
        public Span<byte> AsSpan() => Bytes;

        protected Packet() { }

        protected Packet(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}
