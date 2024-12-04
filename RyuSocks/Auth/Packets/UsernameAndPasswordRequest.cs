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

using RyuSocks.Packets;
using System;

namespace RyuSocks.Auth.Packets
{
    public partial class UsernameAndPasswordRequest : Packet
    {
        private const int MinimumPacketLength = 4;
        private const int MaximumPacketLength = 513;

        [PacketField(0)]
        public partial byte Version { get; set; }

        [PacketField(1)]
        public partial byte UsernameLength { get; set; }

        [PacketField(2, LengthMember = nameof(UsernameLength), MaxLength = 0xFF)]
        public partial string Username { get; set; }

        [PacketField(nameof(PasswordLengthOffset))]
        public partial byte PasswordLength { get; set; }

        [PacketField(nameof(PasswordOffset), LengthMember = nameof(PasswordLength), MaxLength = 0xFF)]
        public partial string Password { get; set; }

        private int PasswordLengthOffset => 2 + UsernameLength;
        private int PasswordOffset => PasswordLengthOffset + 1;

        public UsernameAndPasswordRequest(byte[] packetBytes) : base(packetBytes)
        {
            if (Bytes.Length is < MinimumPacketLength or > MaximumPacketLength)
            {
                throw new ArgumentOutOfRangeException(
                    $"Packet length is invalid: {Bytes.Length} (Expected: {MinimumPacketLength} <= length <= {MaximumPacketLength})");
            }
        }

        public UsernameAndPasswordRequest(string username, string password)
        {
            int packetLength = username.Length + password.Length + 4;

            if (packetLength < MaximumPacketLength)
            {
                Bytes = new byte[packetLength];
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Packet length is invalid: {packetLength} (Expected: {MinimumPacketLength} <= length <= {MaximumPacketLength})");
            }

            Version = AuthConsts.UsernameAndPasswordVersion;
            Username = username;
            Password = password;
        }

        public override void Validate()
        {
            if (Bytes.Length is < MinimumPacketLength or > MaximumPacketLength)
            {
                throw new ArgumentOutOfRangeException($"Packet length is invalid: {Bytes.Length} (Expected: {MinimumPacketLength} <= length <= {MaximumPacketLength})");
            }

            if (Version != AuthConsts.UsernameAndPasswordVersion)
            {
                throw new InvalidOperationException($"{nameof(Version)} is invalid: {Version:X} (Expected: {AuthConsts.UsernameAndPasswordVersion:X})");
            }

            if (string.IsNullOrEmpty(Username))
            {
                throw new InvalidOperationException($"{nameof(Username)} can't be null or empty.");
            }

            if (string.IsNullOrEmpty(Password))
            {
                throw new InvalidOperationException($"{nameof(Password)} can't be null or empty.");
            }
        }
    }
}
