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

namespace RyuSocks.Packets
{
    public abstract partial class EndpointPacket : Packet
    {
        private const byte MinimumPacketLength = 8;
        private const byte Ipv4PacketLength = 10;
        private const byte Ipv6PacketLength = 22;
        private const byte MinimumDomainNameLength = byte.MinValue + 1;
        private const byte MaximumDomainNameLength = byte.MaxValue;

        [PacketField(3, ValidationMethod = nameof(ResizeIfNecessary))]
        public partial AddressType AddressType { get; set; }

        [PacketField(4, LengthMember = nameof(AddressSize))]
        protected partial IPAddress Address { get; set; }

        [PacketField(4, ValidationMethod = nameof(ResizeForDomainNameIfNecessary))]
        protected partial byte DomainNameLength { get; set; }

        [PacketField(5, LengthMember = nameof(DomainNameLength), MinLength = MinimumDomainNameLength, MaxLength = MaximumDomainNameLength, ValidationMethod = nameof(EnsureDomainNameType))]
        protected partial string DomainName { get; set; }

        [PacketField(nameof(PortOffset), IsBigEndian = true)]
        protected partial ushort Port { get; set; }

        public ProxyEndpoint ProxyEndpoint => AddressType == AddressType.DomainName
            ? new ProxyEndpoint(new DnsEndPoint(DomainName, Port))
            : new ProxyEndpoint(new IPEndPoint(Address, Port));

        protected int PacketLength => AddressType switch
        {
            AddressType.Ipv4Address => Ipv4PacketLength,
            AddressType.DomainName => 7 + DomainNameLength,
            AddressType.Ipv6Address => Ipv6PacketLength,
            _ => throw new ArgumentOutOfRangeException(nameof(AddressType)),
        };

        private int AddressSize => AddressType switch
        {
            AddressType.Ipv4Address => 4,
            AddressType.Ipv6Address => 16,
            _ => throw new InvalidOperationException(
                $"Can't get address size for {nameof(Types.AddressType)} {AddressType}."),
        };

        private int PortOffset => AddressType switch
        {
            AddressType.Ipv4Address => Ipv4PacketLength - 2,
            AddressType.DomainName => 5 + DomainNameLength,
            AddressType.Ipv6Address => Ipv6PacketLength - 2,
            _ => throw new ArgumentOutOfRangeException(nameof(AddressType)),
        };

        protected EndpointPacket(byte[] bytes) : base(bytes)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bytes.Length, MinimumPacketLength, nameof(bytes));
        }

        protected EndpointPacket(IPEndPoint endpoint)
        {
            Bytes = endpoint.AddressFamily switch
            {
                AddressFamily.InterNetwork => new byte[Ipv4PacketLength],
                AddressFamily.InterNetworkV6 => new byte[Ipv6PacketLength],
                _ => throw new ArgumentException(
                    $"Unsupported {nameof(AddressFamily)}: {endpoint.AddressFamily}", nameof(endpoint)),
            };

            AddressType = Bytes.Length == Ipv4PacketLength ? AddressType.Ipv4Address : AddressType.Ipv6Address;
            Address = endpoint.Address;
            Port = (ushort)endpoint.Port;
        }

        protected EndpointPacket(DnsEndPoint endpoint)
        {
            if (endpoint.Host.Length is < MinimumDomainNameLength or > MaximumDomainNameLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endpoint),
                    $"Host length must be between {MinimumDomainNameLength} and {MaximumDomainNameLength}.");
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
                throw new ArgumentOutOfRangeException(
                    nameof(endpoint),
                    $"Host length must be between {MinimumDomainNameLength} and {MaximumDomainNameLength}.");
            }

            Bytes = endpoint.Type switch
            {
                AddressType.Ipv4Address => new byte[Ipv4PacketLength],
                AddressType.DomainName => Bytes = new byte[7 + endpoint.DomainName.Length],
                AddressType.Ipv6Address => new byte[Ipv6PacketLength],
                _ => throw new ArgumentException(
                    $"Invalid {nameof(Types.AddressType)}: {endpoint.Type}", nameof(endpoint)),
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

        // ReSharper disable once UnusedParameter.Local
        private void EnsureDomainNameType(string value = "")
        {
            if (AddressType != AddressType.DomainName)
            {
                throw new InvalidOperationException(
                    $"Can't get {nameof(DomainNameLength)} for {nameof(Types.AddressType)} {AddressType}.");
            }
        }

        private void ResizeIfNecessary(AddressType value = 0)
        {
            if (value == 0)
            {
                return;
            }

            if (AddressType is AddressType.Ipv4Address or AddressType.Ipv6Address)
            {
                int requiredLength = PacketLength;
                if (Bytes.Length != requiredLength)
                {
                    byte[] resizeBytes = Bytes;
                    Array.Resize(ref resizeBytes, requiredLength);
                    Bytes = resizeBytes;
                }
            }
        }

        private void ResizeForDomainNameIfNecessary(byte value = 0)
        {
            EnsureDomainNameType();

            if (value == 0)
            {
                return;
            }

            int requiredLength = 7 + value;
            if (Bytes.Length != requiredLength)
            {
                byte[] resizeBytes = Bytes;
                Array.Resize(ref resizeBytes, requiredLength);
                Bytes = resizeBytes;
            }
        }

        public override void Validate()
        {
            if (Bytes.Length < MinimumPacketLength)
            {
                throw new InvalidOperationException($"Invalid packet length: {Bytes.Length} (Expected: >= {MinimumPacketLength})");
            }

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                    if (Bytes.Length != Ipv4PacketLength)
                    {
                        throw new InvalidOperationException($"Invalid packet length: {Bytes.Length} (Expected: {Ipv4PacketLength})");
                    }

                    if (Address == null)
                    {
                        throw new InvalidOperationException($"{nameof(Address)} could not be parsed.");
                    }

                    break;
                case AddressType.Ipv6Address:
                    if (Bytes.Length != Ipv6PacketLength)
                    {
                        throw new InvalidOperationException($"Invalid packet length: {Bytes.Length} (Expected: {Ipv6PacketLength})");
                    }

                    if (Address == null)
                    {
                        throw new InvalidOperationException($"{nameof(Address)} could not be parsed.");
                    }

                    break;
                case AddressType.DomainName:
                    if (DomainName.Length is < MinimumDomainNameLength or > MaximumDomainNameLength)
                    {
                        throw new InvalidOperationException($"Invalid {nameof(DomainName)} length: {DomainName.Length} (Expected: {MinimumDomainNameLength} <= length <= {MaximumDomainNameLength})");
                    }

                    if (DomainNameLength != DomainName.Length)
                    {
                        throw new InvalidOperationException($"{nameof(DomainNameLength)} is not equal to the length of {nameof(DomainName)}: {DomainNameLength} != {DomainName.Length}");
                    }

                    break;
                default:
                    throw new InvalidOperationException($"{nameof(AddressType)} is invalid: {AddressType}");
            }
        }
    }
}
