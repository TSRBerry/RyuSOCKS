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
    public partial class UsernameAndPasswordResponse : Packet
    {
        [PacketField(0)]
        public partial byte Version { get; set; }

        [PacketField(1)]
        public partial byte Status { get; set; }

        public UsernameAndPasswordResponse()
        {
            Bytes = new byte[2];
        }

        public UsernameAndPasswordResponse(byte status) : this()
        {
            Status = status;
        }

        public UsernameAndPasswordResponse(byte[] packetBytes) : base(packetBytes) { }

        public override void Validate()
        {
            if (Version != AuthConsts.UsernameAndPasswordVersion)
            {
                throw new InvalidOperationException($"{nameof(Version)} is invalid: {Version:X} (Expected: {AuthConsts.UsernameAndPasswordVersion:X})");
            }
        }
    }
}
