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
    public class MethodSelectionRequest : Packet
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

        public byte NumOfMethods
        {
            get
            {
                return Bytes[1];
            }
            set
            {
                Bytes[1] = value;
            }
        }

        public AuthMethod[] Methods
        {
            get
            {
                return Bytes[2..(2 + NumOfMethods)].Cast<AuthMethod>().ToArray();

            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 0xFF);
                NumOfMethods = (byte)value.Length;
                value.Cast<byte>().ToArray().CopyTo(Bytes.AsSpan(2, value.Length));
            }
        }

        public MethodSelectionRequest(byte[] packetBytes)
        {
            Bytes = packetBytes;
        }

        public MethodSelectionRequest(AuthMethod[] methods)
        {
            Bytes = new byte[2 + methods.Length];
            Methods = methods;
        }

        public override void Validate()
        {
            if (Bytes.Length < 3)
            {
                throw new InvalidOperationException($"Packet length is too short: {Bytes.Length} (Expected: >= 3)");
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
