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

        protected CommandPacket(byte[] bytes) : base(bytes) { }
        protected CommandPacket(IPEndPoint endpoint) : base(endpoint) { }
        protected CommandPacket(DnsEndPoint endpoint) : base(endpoint) { }
        protected CommandPacket(ProxyEndpoint endpoint) : base(endpoint) { }
        protected CommandPacket() { }

        public override void Validate()
        {
            // Minimum length: VER(1) + CMD(1) + RSV(1) + 0x03 + 0x01 + FQDN(1) + PORT(2) = 8
            const int MinimumLength = 8;
            // Maximum length: VER(1) + CMD(1) + RSV(1) + 0x03 + 0xFF + FQDN(0xFF) + PORT(2) = 262
            const int MaximumLength = 262;

            if (Bytes.Length is < MinimumLength or > MaximumLength)
            {
                throw new InvalidOperationException($"Invalid packet length: {Bytes.Length} (Expected: {MinimumLength} <= length <= {MaximumLength})");
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
