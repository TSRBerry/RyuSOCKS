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
    public class UdpHeader : EndpointPacket
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

        protected string DestinationDomainName
        {
            get => DomainName;
            set => DomainName = value;
        }

        protected ushort DestinationPort
        {
            get => Port;
            set => Port = value;
        }

        public override void Validate()
        {
            if (Reserved != 0)
            {
                throw new InvalidOperationException($"Reserved field is not 0: {Reserved}");
            }
        }
    }
}
