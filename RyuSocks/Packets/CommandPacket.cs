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

using RyuSocks.Utils;
using System;
using System.Net;

namespace RyuSocks.Packets
{
    public abstract class CommandPacket : EndpointPacket
    {
        public byte Version
        {
            get
            {
                return Bytes[0];
            }
            set
            {
                Bytes[0] = value;
            }
        }

        // ProxyCommand or ReplyField

        public byte Reserved
        {
            get
            {
                return Bytes[2];
            }
            set
            {
                Bytes[2] = value;
            }
        }

        // AddressType

        // Address

        // Port

        protected CommandPacket(byte[] packetBytes) : base(packetBytes) { }
        protected CommandPacket(IPEndPoint endpoint) : base(endpoint) { }
        protected CommandPacket(DnsEndPoint endpoint) : base(endpoint) { }
        protected CommandPacket() { }

        public override void Validate()
        {
            if (Bytes.Length < 8)
            {
                throw new InvalidOperationException($"Packet length is too short: {Bytes.Length} (Expected: >= 8)");
            }

            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException(
                    $"{nameof(Version)} is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }

            if (Reserved != 0)
            {
                throw new InvalidOperationException($"{nameof(Reserved)} must be 0.");
            }
        }
    }
}
