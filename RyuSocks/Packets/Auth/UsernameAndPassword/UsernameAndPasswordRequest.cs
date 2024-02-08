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
using System.Text;

namespace RyuSocks.Packets.Auth.UsernameAndPassword
{
    public class UsernameAndPasswordRequest : IPacket
    {
        public byte Version { get; set; }
        public byte UsernameLength;
        public string Username;
        public byte PasswordLength;
        public string Password;

        public void FromArray(byte[] array)
        {
            Version = array[0];
            UsernameLength = array[1];
            Username = Encoding.ASCII.GetString(array[2..(2 + UsernameLength)]);
            PasswordLength = array[2 + UsernameLength];
            Password = Encoding.ASCII.GetString(
                array[(2 + UsernameLength + 1)..((2 + UsernameLength + 1) + PasswordLength)]);
        }

        public byte[] ToArray()
        {
            byte[] array = new byte[3 + PasswordLength + UsernameLength];
            array[0] = Version;
            array[1] = UsernameLength;
            char[] passwordInChars = Password.ToCharArray();
            char[] usernameInChars = Username.ToCharArray();

            // This can probably be done more proficiently, but I don't know how
            for (int i = 0; i < UsernameLength; i++)
            {
                array[2 + i] = (byte)usernameInChars[i];
            }

            for (int i = 0; i < PasswordLength; i++)
            {
                array[3 + UsernameLength + i] = (byte)passwordInChars[i];
            }

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
