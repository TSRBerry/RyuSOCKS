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
using System.Net;

namespace RyuSocks.Packets
{
    public class UdpPacket : EndpointPacket
    {
        public ushort Reserved
        {
            get
            {
                return BitConverter.ToUInt16(Bytes.AsSpan(0, 2));
            }
            set
            {
                BitConverter.GetBytes(value).CopyTo(Bytes.AsSpan(0, 2));
            }
        }

        public byte Fragment
        {
            get
            {
                return Bytes[2];
            }
            set
            {
                Bytes[2] = value;
            }
        }

        // AddressType

        public IPAddress DestinationAddress
        {
            get => Address;
            set => Address = value;
        }

        public string DestinationDomainName
        {
            get => DomainName;
            set => DomainName = value;
        }

        public ushort DestinationPort
        {
            get => Port;
            set => Port = value;
        }

        public Span<byte> UserData => Bytes.AsSpan(GetEndpointPacketLength());

        public UdpPacket(byte[] packetBytes) : base(packetBytes) { }

        public UdpPacket(IPEndPoint endpoint, int payloadLength) : base(endpoint)
        {
            byte[] baseBytes = Bytes;
            Array.Resize(ref baseBytes, Bytes.Length + payloadLength);
            Bytes = baseBytes;
        }

        public UdpPacket(DnsEndPoint endpoint, int payloadLength) : base(endpoint)
        {
            byte[] baseBytes = Bytes;
            Array.Resize(ref baseBytes, Bytes.Length + payloadLength);
            Bytes = baseBytes;
        }

        public override void Validate()
        {
            if (Bytes.Length < 8)
            {
                throw new InvalidOperationException($"Packet length is too short: {Bytes.Length} (Expected: >= 8)");
            }

            if (Reserved != 0)
            {
                throw new InvalidOperationException($"${nameof(Reserved)} must be 0.");
            }
        }
    }
}
