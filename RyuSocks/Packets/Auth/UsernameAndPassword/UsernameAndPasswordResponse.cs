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
    public class UsernameAndPasswordResponse : IPacket
    {
        public byte Version { get; set; }
        public ReplyField Status { get; set; }

        public UsernameAndPasswordResponse(byte version, ReplyField status)
        {
            Version = version;
            Status = status;
        }

        public void FromArray(byte[] array)
        {
            Version = array[0];
            Status = (ReplyField)array[1];
        }

        public byte[] ToArray()
        {
            byte[] array = new byte[2];
            array[0] = Version;
            array[1] = (byte)Status;
            return array;
        }

        public void Verify()
        {
            if (Version != 0x01)
            {
                throw new AuthenticationException("The package is of the wrong type.");
            }
        }
    }
}
