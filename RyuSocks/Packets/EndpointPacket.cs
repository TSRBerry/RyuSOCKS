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
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RyuSocks.Packets
{
    public abstract class EndpointPacket : Packet
    {
        private const byte MinimumDomainNameLength = byte.MinValue + 1;
        private const byte MaximumDomainNameLength = byte.MaxValue;

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
                    AddressType.DomainName => throw new InvalidOperationException($"Can't get {nameof(Address)} for {AddressType.DomainName}."),
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
                        throw new InvalidOperationException($"Can't set {nameof(Address)} for {AddressType.DomainName}.");
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
                        throw new InvalidOperationException($"Can't get {nameof(DomainName)} for {AddressType.Ipv4Address}.");
                    case AddressType.DomainName:
                        return Encoding.ASCII.GetString(Bytes, 5, Bytes[4]);
                    case AddressType.Ipv6Address:
                        throw new InvalidOperationException($"Can't get {nameof(DomainName)} for {AddressType.Ipv6Address}.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(AddressType));
                }
            }
            set
            {
                switch (AddressType)
                {
                    case AddressType.Ipv4Address:
                        throw new InvalidOperationException($"Can't set {nameof(DomainName)} for {AddressType.Ipv4Address}.");
                    case AddressType.DomainName:
                        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, MaximumDomainNameLength);
                        Bytes[4] = (byte)value.Length;
                        Encoding.ASCII.GetBytes(value, Bytes.AsSpan(5, Bytes[4]));
                        return;
                    case AddressType.Ipv6Address:
                        throw new InvalidOperationException($"Can't set {nameof(DomainName)} for {AddressType.Ipv6Address}.");
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

        public ProxyEndpoint ProxyEndpoint => AddressType == AddressType.DomainName
            ? new ProxyEndpoint(new DnsEndPoint(DomainName, Port))
            : new ProxyEndpoint(new IPEndPoint(Address, Port));

        protected EndpointPacket(byte[] bytes) : base(bytes) { }

        protected EndpointPacket(IPEndPoint endpoint)
        {
            Bytes = endpoint.AddressFamily switch
            {
                AddressFamily.InterNetwork => new byte[10],
                AddressFamily.InterNetworkV6 => new byte[22],
                _ => throw new ArgumentException($"Unsupported {nameof(AddressFamily)}: {endpoint.AddressFamily}", nameof(endpoint)),
            };

            AddressType = Bytes.Length == 10 ? AddressType.Ipv4Address : AddressType.Ipv6Address;
            Address = endpoint.Address;
            Port = (ushort)endpoint.Port;
        }

        protected EndpointPacket(DnsEndPoint endpoint)
        {
            if (endpoint.Host.Length is < MinimumDomainNameLength or > MaximumDomainNameLength)
            {
                throw new ArgumentException($"Length of Host must be between {MinimumDomainNameLength} and {MaximumDomainNameLength}.", nameof(endpoint));
            }

            Bytes = new byte[7 + endpoint.Host.Length];
            AddressType = AddressType.DomainName;
            DomainName = endpoint.Host;
            Port = (ushort)endpoint.Port;
        }

        protected EndpointPacket(ProxyEndpoint endpoint)
        {
            if (endpoint.Type == AddressType.DomainName && endpoint.DomainName.Length is < MinimumDomainNameLength or > MaximumDomainNameLength)
            {
                throw new ArgumentException($"Length of Host must be between {MinimumDomainNameLength} and {MaximumDomainNameLength}.", nameof(endpoint));
            }

            Bytes = endpoint.Type switch
            {
                AddressType.Ipv4Address => new byte[10],
                AddressType.DomainName => Bytes = new byte[7 + endpoint.DomainName.Length],
                AddressType.Ipv6Address => new byte[22],
                _ => throw new ArgumentException($"Invalid {nameof(Types.AddressType)}: {endpoint.Type}", nameof(endpoint)),
            };

            AddressType = endpoint.Type;
            if (endpoint.Type == AddressType.DomainName)
            {
                DomainName = endpoint.DomainName;
                Port = endpoint.Port;
            }
            else
            {
                Address = endpoint.Addresses.Single();
                Port = endpoint.Port;
            }
        }

        protected EndpointPacket()
        {
            Bytes = new byte[10];
            AddressType = AddressType.Ipv4Address;
        }

        protected int GetEndpointPacketLength()
        {
            return AddressType switch
            {
                AddressType.Ipv4Address => 10,
                AddressType.DomainName => 7 + DomainName.Length,
                AddressType.Ipv6Address => 22,
                _ => throw new ArgumentOutOfRangeException(nameof(AddressType)),
            };
        }

        private Span<byte> GetPortSpan()
        {
            return AddressType switch
            {
                AddressType.Ipv4Address => Bytes.AsSpan(8, 2),
                AddressType.DomainName => Bytes.AsSpan(5 + Bytes[4], 2),
                AddressType.Ipv6Address => Bytes.AsSpan(20, 2),
                _ => throw new ArgumentOutOfRangeException(nameof(AddressType)),
            };
        }
    }
}
