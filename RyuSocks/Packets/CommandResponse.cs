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

using RyuSocks.Types;
using System.Net;

namespace RyuSocks.Packets
{
    public class CommandResponse : CommandPacket
    {
        // Version

        public ReplyField ReplyField
        {
            get
            {
                return (ReplyField)Bytes[1];
            }
            set
            {
                Bytes[1] = (byte)value;
            }
        }

        // Reserved

        // AddressType

        public IPAddress BoundAddress
        {
            get => Address;
            set => Address = value;
        }

        public string BoundDomainName
        {
            get => DomainName;
            set => DomainName = value;
        }

        public ushort BoundPort
        {
            get => Port;
            set => Port = value;
        }

        public CommandResponse(byte[] packetBytes) : base(packetBytes) { }
        public CommandResponse(IPEndPoint endpoint) : base(endpoint) { }
        public CommandResponse(DnsEndPoint endpoint) : base(endpoint) { }
        public CommandResponse() { }
    }
}
