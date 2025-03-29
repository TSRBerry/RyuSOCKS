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

using RyuSocks.Auth;
using RyuSocks.Utils;
using System;
using System.Linq;

namespace RyuSocks.Packets
{
    public partial class MethodSelectionRequest : Packet
    {
        [PacketField(0)]
        public partial byte Version { get; set; }

        [PacketField(1)]
        public partial byte NumOfMethods { get; set; }

        [PacketField(2, LengthMember = nameof(NumOfMethods), MaxLength = 0xFF, AssumeGeneratedEnumType = nameof(Byte))]
        public partial AuthMethod[] Methods { get; set; }

        public MethodSelectionRequest(byte[] bytes) : base(bytes) { }

        public MethodSelectionRequest(AuthMethod[] methods)
        {
            Bytes = new byte[2 + methods.Length];
            Methods = methods;
        }

        public override void Validate()
        {
            // Minimum length: VER(1) + 0x01 + METHODS(1) = 3
            const int MinimumLength = 3;
            // Maximum length: VER(1) + 0xFF + METHODS(0xFF) = 257
            // Technically 256 since NoAcceptableMethods could be excluded, but we'll catch that later.
            const int MaximumLength = 257;

            if (Bytes.Length is < MinimumLength or > MaximumLength)
            {
                throw new InvalidOperationException($"Invalid packet length: {Bytes.Length} (Expected: {MinimumLength} <= length <= {MaximumLength})");
            }

            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException($"{nameof(Version)} is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }

            if (Methods.Contains(AuthMethod.NoAcceptableMethods))
            {
                throw new InvalidOperationException($"{AuthMethod.NoAcceptableMethods} can't be requested.");
            }
        }
    }
}
