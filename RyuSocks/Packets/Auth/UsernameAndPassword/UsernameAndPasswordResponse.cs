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

namespace RyuSocks.Packets.Auth.UsernameAndPassword
{
    public class UsernameAndPasswordResponse : Packet
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

        public byte Status
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

        public UsernameAndPasswordResponse()
        {
            Bytes = new byte[2];
        }

        public UsernameAndPasswordResponse(byte[] packetBytes)
        {
            Bytes = packetBytes;
        }

        public override void Validate()
        {
            if (Version != Constants.UsernameAndPasswordVersion)
            {
                throw new InvalidOperationException($"${nameof(Version)} is invalid: {Version:X} (Expected: {Constants.UsernameAndPasswordVersion})");
            }
        }
    }
}
