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
using RyuSocks.Types;
using RyuSocks.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;

namespace RyuSocks.Test.Packets
{
    [ExcludeFromCodeCoverage]
    public abstract class CommandPacketTests<T> where T : CommandPacket
    {
        private const string VeryLongInvalidTestDomainName = "abcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc.local";

        private readonly Func<byte[], T> _constructor;

        protected CommandPacketTests(Func<byte[], T> constructorFunc)
        {
            _constructor = constructorFunc;
        }

        [Theory]
        [InlineData(0x00, 0x01, 0x00, AddressType.DomainName, "test.local", 1042, false)]
        [InlineData(0x01, 0x01, 0x00, AddressType.DomainName, "test.local", 1242, false)]
        [InlineData(ProxyConsts.Version, 0x00, 0x2F, AddressType.DomainName, "test.local", 42, false)]
        [InlineData(ProxyConsts.Version, 0x00, 0x00, AddressType.Ipv6Address, "test.local", 66, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, (AddressType)0xFF, "test.local", 21, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, AddressType.DomainName, "", 1222, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, AddressType.DomainName, VeryLongInvalidTestDomainName, 1111, false)]
        [InlineData(ProxyConsts.Version, 0xFF, 0x00, AddressType.DomainName, "test.local", 0, true)]
        [InlineData(ProxyConsts.Version, 0x01, 0x00, AddressType.DomainName, "test.local", 1042, true)]
        public void Validate_ThrowsOnInvalidValue_DomainName(byte version, byte commandOrReplyField, byte reserved, AddressType addressType, string domainName, ushort port, bool isValidInput)
        {
            // Construct the packet manually to skip the checks in the constructor
            byte[] packetBytes = [
                // Version
                version,
                // Command or ReplyField
                commandOrReplyField,
                // Reserved
                reserved,
                // Address type
                (byte)addressType,
                // Address
                // Port
            ];
            Array.Resize(ref packetBytes, packetBytes.Length + 1 + (domainName.Length > 0 ? domainName.Length : 1) + 2);
            // Address
            packetBytes[4] = (byte)Encoding.ASCII.GetByteCount(domainName);
            Encoding.ASCII.GetBytes(domainName).CopyTo(packetBytes.AsSpan(5));
            // Port
            BitConverter.GetBytes(port).Reverse().ToArray().CopyTo(packetBytes.AsSpan(packetBytes.Length - 2));

            T packet = _constructor(packetBytes);

            if (!isValidInput)
            {
                Assert.ThrowsAny<Exception>(() => packet.Validate());
                return;
            }

            packet.Validate();
        }

        [Theory]
        [InlineData(0x00, 0x01, 0x00, AddressType.Ipv6Address, "2001:db8::aaaa:c0c0:4444", 1042, false, false)]
        [InlineData(0x00, 0x01, 0x00, AddressType.Ipv4Address, "10.0.0.122", 1242, true, false)]
        [InlineData(0x01, 0x01, 0x00, AddressType.Ipv6Address, "2001:db8::aaaa:c0c0:4444", 1042, false, false)]
        [InlineData(0xAF, 0x01, 0x00, AddressType.Ipv4Address, "10.0.0.122", 1242, true, false)]
        [InlineData(ProxyConsts.Version, 0x00, 0x7F, AddressType.Ipv6Address, "2001:db8::aaaa:c0c0:4444", 1042, false, false)]
        [InlineData(ProxyConsts.Version, 0x00, 0xAA, AddressType.Ipv4Address, "10.0.0.122", 1242, true, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, AddressType.DomainName, "2001:db8::aaaa:c0c0:4444", 1042, false, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, AddressType.DomainName, "10.0.0.122", 1242, true, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, (AddressType)0xFF, "2001:db8::aaaa:c0c0:4444", 1042, false, false)]
        [InlineData(ProxyConsts.Version, 0x22, 0x00, (AddressType)0xFF, "10.0.0.122", 1242, true, false)]
        [InlineData(ProxyConsts.Version, 0xCC, 0x00, AddressType.Ipv6Address, "2001:db8::aaaa:c0c0:4444", 0, false, true)]
        [InlineData(ProxyConsts.Version, 0xCC, 0x00, AddressType.Ipv4Address, "10.0.0.122", 0, true, true)]
        [InlineData(ProxyConsts.Version, 0xCC, 0x00, AddressType.Ipv6Address, "2001:db8::ffff:0001", 2, false, true)]
        [InlineData(ProxyConsts.Version, 0xCC, 0x00, AddressType.Ipv4Address, "0.0.0.0", 1042, true, true)]
        [InlineData(ProxyConsts.Version, 0xFF, 0x00, AddressType.Ipv6Address, "2001:db8::aaaa:c0c0:4444", 2222, false, true)]
        [InlineData(ProxyConsts.Version, 0xFF, 0x00, AddressType.Ipv4Address, "10.0.0.122", 1111, true, true)]
        public void Validate_ThrowsOnInvalidValue_IPAddress(byte version, byte commandOrReplyField, byte reserved, AddressType addressType, string ipAddress, ushort port, bool isIpv4, bool isValidInput)
        {
            // Construct the packet manually to skip the checks in the constructor
            IPAddress address = IPAddress.Parse(ipAddress);
            byte[] packetBytes = [
                // Version
                version,
                // Command or ReplyField
                commandOrReplyField,
                // Reserved
                reserved,
                // Address type
                (byte)addressType,
                // Address
                // Port
            ];
            Array.Resize(ref packetBytes, packetBytes.Length + (isIpv4 ? 4 : 16) + 2);
            // Address
            bool addressWritten = address.TryWriteBytes(packetBytes.AsSpan(4), out _);
            Assert.True(addressWritten);
            // Port
            BitConverter.GetBytes(port).Reverse().ToArray().CopyTo(packetBytes.AsSpan(packetBytes.Length - 2));

            T packet = _constructor(packetBytes);

            if (!isValidInput)
            {
                Assert.ThrowsAny<Exception>(() => packet.Validate());
                return;
            }

            packet.Validate();
        }
    }
}
