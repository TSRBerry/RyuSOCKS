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

using System.Security.Authentication;

namespace RyuSocks.Packets.Auth.UsernameAndPassword
{
    public class UsernameAndPasswordResponse : Packet
    {

        public UsernameAndPasswordResponse()
        {
            Bytes = new byte[2];
        }

        public UsernameAndPasswordResponse(byte[] packetBytes, byte version, byte status)
        {
            Bytes = packetBytes;

            Version = version;
            Status = status;
        }

        public byte Version { get; set; }
        public byte Status { get; set; }

        public void FromArray(byte[] array)
        {
            Version = array[0];
            Status = array[1];
        }

        public byte[] ToArray()
        {
            byte[] array = [Version, (byte)Status];
            return array;
        }

        public override void Validate()
        {
            if (Version != 0x01)
            {
                throw new AuthenticationException("The package is of the wrong type.");
            }
        }
    }
}
