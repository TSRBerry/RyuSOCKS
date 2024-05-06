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

using RyuSocks.Packets.Auth;
using System;
using System.Text;

namespace RyuSocks.Packets.Auth.UsernameAndPassword
{
    public class UsernameAndPasswordRequest : Packet
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

        public byte UsernameLength
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

        public string Username
        {
            get
            {
                return Encoding.ASCII.GetString(Bytes.AsSpan(2, UsernameLength));
            }
            set
            {
                Encoding.ASCII.GetBytes(value).CopyTo(Bytes.AsSpan(2, UsernameLength));
            }
        }

        public byte PasswordLength
        {
            get
            {
                return Bytes[3 + UsernameLength];
            }
            set
            {
                Bytes[3 + UsernameLength] = value;
            }
        }

        public string Password
        {
            get
            {
                return Encoding.ASCII.GetString(Bytes.AsSpan(4 + UsernameLength, PasswordLength));
            }
            set
            {
                Encoding.ASCII.GetBytes(value).CopyTo(Bytes.AsSpan(4 + UsernameLength, PasswordLength));
            }
        }

        public UsernameAndPasswordRequest(byte[] packetBytes)
        {
            Bytes = packetBytes;
        }

        public UsernameAndPasswordRequest(string username, string password)
        {
            Version = Constants.UaPVersion;
            UsernameLength = (byte)username.Length;
            Username = username;
            PasswordLength = (byte)password.Length;
            Password = password;
        }

        public override void Validate()
        {
            if (Version != Constants.UaPVersion)
            {
                throw new InvalidOperationException($"${nameof(Version)} is invalid: {Version:X} (Expected: {Constants.UaPVersion})");
            }
        }
    }
}