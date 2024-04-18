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
using System.Linq;
using System.Security.Authentication;
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
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 0xFF);
                UsernameLength = (byte)value.Length;
                Encoding.ASCII.GetBytes(value).CopyTo(Bytes.AsSpan(2, UsernameLength));
            }
        }

        public byte PasswordLength
        {
            get
            {
                return Bytes[2 + UsernameLength];
            }
            set
            {
                Bytes[2 + UsernameLength] = value;
            }
        }

        public string Password
        {
            get
            {
                return Encoding.ASCII.GetString(Bytes.AsSpan(3 + UsernameLength, PasswordLength));
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 0xFF);
                PasswordLength = (byte)value.Length;
                Encoding.ASCII.GetBytes(value).CopyTo(Bytes.AsSpan(3 + UsernameLength, PasswordLength));
            }
        }

        public UsernameAndPasswordRequest(byte[] packetBytes)
        {
            Bytes = packetBytes;
            Version = Bytes[0];
            UsernameLength = Bytes[1];
            try
            { Username = Encoding.ASCII.GetString(Bytes[2..(2 + UsernameLength)]); }
            catch (Exception e)
            {
                throw new InvalidOperationException($"UsernameLength and actual Username length do not match: {Bytes[1]:X} (throws {e})");
            }
            PasswordLength = Bytes[2 + UsernameLength];
            try
            {
                Password = Encoding.ASCII.GetString(
                    Bytes[(2 + UsernameLength + 1)..((2 + UsernameLength + 1) + PasswordLength)]);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"PasswordLength and actual Password length do not match: {Bytes[1]:X} (throws {e})");
            }
        }

        public UsernameAndPasswordRequest(string username, string password)
        {
            Bytes = new byte[4 + username.Length + password.Length];
            Version = Constants.UaPVersion;
            UsernameLength = (byte)username.Length;
            Username = username;
            PasswordLength = (byte)password.Length;
            Password = password;
        }

        public override void Validate()
        {
            const int MinLength = 4;
            const int MaxLength = 513;
            if (Bytes.Length is < MinLength or > MaxLength)
            {
                throw new InvalidOperationException($"$Package has wrong Length: {Bytes.Length:X} (Expected: >= {MinLength} || <= {MaxLength})");
            }
            if (Version != Constants.UaPVersion)
            {
                throw new InvalidOperationException(
                    $"${nameof(Version)} is invalid: {Version:X} (Expected: {Constants.UaPVersion})");
            }

            if (Username == null)
            {
                throw new InvalidOperationException($"$Username is invalid: {Username} (Expected: Literally Anything but null)");
            }

            if (Password == null)
            {
                throw new InvalidOperationException($"Password is invalid: {Password} (No Password is not allowed)");
            }
        }
    }
}
