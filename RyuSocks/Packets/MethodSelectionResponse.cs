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

namespace RyuSocks.Packets
{
    public class MethodSelectionResponse : Packet
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

        public AuthMethod Method
        {
            get
            {
                return (AuthMethod)Bytes[1];
            }
            set
            {
                Bytes[1] = (byte)value;
            }
        }

        public MethodSelectionResponse(byte[] packetBytes)
        {
            Bytes = packetBytes;
        }

        public MethodSelectionResponse(AuthMethod method)
        {
            Bytes = new byte[2];
            Method = method;
        }

        public override void Validate()
        {
            if (Bytes.Length < 2)
            {
                throw new InvalidOperationException($"Packet length is too short: {Bytes.Length} (Expected: >= 2)");
            }

            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException($"{nameof(Version)} is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }
        }
    }
}
