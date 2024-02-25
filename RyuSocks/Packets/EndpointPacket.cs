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
using System.Text;

namespace RyuSocks.Packets
{
    public abstract class EndpointPacket : Packet
    {
        public AddressType AddressType
        {
            get
            {
                return (AddressType)Bytes[3];
            }
            set
            {
                Bytes[3] = (byte)value;
            }
        }

        protected IPAddress Address
        {
            get
            {
                return AddressType switch
                {
                    AddressType.Ipv4Address => new IPAddress(Bytes[4..8]),
                    AddressType.DomainName => throw new InvalidOperationException(),
                    AddressType.Ipv6Address => new IPAddress(Bytes[4..20]),
                    _ => throw new ArgumentOutOfRangeException(nameof(AddressType)),
                };
            }
            set
            {
                switch (AddressType)
                {
                    case AddressType.Ipv4Address:
                        value.GetAddressBytes().CopyTo(Bytes.AsSpan(4, 4));
                        return;
                    case AddressType.DomainName:
                        throw new InvalidOperationException();
                    case AddressType.Ipv6Address:
                        value.GetAddressBytes().CopyTo(Bytes.AsSpan(4, 16));
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        protected string DomainName
        {
            get
            {
                switch (AddressType)
                {
                    case AddressType.Ipv4Address:
                        throw new InvalidOperationException();
                    case AddressType.DomainName:
                        ArgumentOutOfRangeException.ThrowIfGreaterThan(Bytes[4], 255);
                        return Encoding.ASCII.GetString(Bytes, 5, Bytes[4]);
                    case AddressType.Ipv6Address:
                        throw new InvalidOperationException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(AddressType));
                }
            }
            set
            {
                switch (AddressType)
                {
                    case AddressType.Ipv4Address:
                        throw new InvalidOperationException();
                    case AddressType.DomainName:
                        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 255);
                        Bytes[4] = (byte)value.Length;
                        Encoding.ASCII.GetBytes(value, Bytes.AsSpan(5, Bytes[4]));
                        return;
                    case AddressType.Ipv6Address:
                        throw new InvalidOperationException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        protected ushort Port
        {
            get
            {
                Span<byte> portSpan = GetPortSpan();
                portSpan.Reverse();
                return BitConverter.ToUInt16(portSpan);
            }
            set
            {
                byte[] portBytes = BitConverter.GetBytes(value);
                Array.Reverse(portBytes);
                portBytes.CopyTo(GetPortSpan());
            }
        }

        private Span<byte> GetPortSpan()
        {
            return AddressType switch
            {
                AddressType.Ipv4Address => Bytes.AsSpan(9, 2),
                AddressType.DomainName => Bytes.AsSpan(5 + Bytes[4], 2),
                AddressType.Ipv6Address => Bytes.AsSpan(21, 2),
                _ => throw new ArgumentOutOfRangeException(nameof(AddressType)),
            };
        }
    }
}
