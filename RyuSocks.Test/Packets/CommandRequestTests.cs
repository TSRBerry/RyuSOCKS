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

using RyuSocks.Commands;
using RyuSocks.Packets;
using RyuSocks.Test.Utils;
using RyuSocks.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace RyuSocks.Test.Packets
{
    public class CommandRequestTests
    {
        [Theory]
        [StringData(byte.MinValue + 1, byte.MaxValue, 4)]
        [StringData(byte.MinValue, byte.MaxValue + 1)]
        public void Bytes_Size_DnsEndPoint(string domainName)
        {
            // Version: 1 byte
            // Command: 1 byte
            // Reserved: 1 byte
            // Address type: 1 byte
            // Address: 2 - 256 bytes
            // Port: 2 bytes
            // Total: 8 - 262 bytes

            if (domainName.Length == 0)
            {
                Assert.Throws<ArgumentException>(() => new DnsEndPoint(domainName, 0));
                return;
            }

            DnsEndPoint endpoint = new(domainName, 0);

            if (domainName.Length > byte.MaxValue)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new CommandRequest(endpoint));
                return;
            }

            CommandRequest request = new(endpoint);

            Assert.Equal(7 + domainName.Length, request.Bytes.Length);
        }

        [Theory]
        [InlineData("10.0.0.5", 2211, true)]
        [InlineData("2001:db8::abba:c000:1221", 572, false)]
        public void Bytes_Size_IPEndPoint(string ipAddress, ushort port, bool isIpv4)
        {
            // Version: 1 byte
            // Command: 1 byte
            // Reserved: 1 byte
            // Address type: 1 byte
            // Address: 4 or 16 bytes
            // Port: 2 bytes
            // Total: 10 or 22 bytes

            IPEndPoint endpoint = new(IPAddress.Parse(ipAddress), port);
            CommandRequest request = new(endpoint);

            AddressFamily expectedAddressFamily = isIpv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
            int expectedLength = isIpv4 ? 10 : 22;

            Assert.Equal(expectedAddressFamily, endpoint.AddressFamily);
            Assert.Equal(expectedLength, request.Bytes.Length);
        }

        [Theory]
        [InlineData(0x22, ProxyCommand.Connect, "abc.local", 1337)]
        [InlineData(ProxyConsts.Version, ProxyCommand.UdpAssociate, "123.local", 4242)]
        [InlineData(ProxyConsts.Version, (ProxyCommand)byte.MaxValue, "test.local", 1042)]
        public void Bytes_MatchStructure_DnsEndPoint(byte version, ProxyCommand command, string domainName, ushort port)
        {
            byte[] expectedBytes = [
                // Version
                version,
                // Command
                (byte)command,
                // Reserved
                0x0,
                // Address type
                0x03,
                // Address
                // Port
            ];
            Array.Resize(ref expectedBytes, expectedBytes.Length + 1 + domainName.Length + 2);
            // Address
            expectedBytes[4] = (byte)Encoding.ASCII.GetByteCount(domainName);
            Encoding.ASCII.GetBytes(domainName).CopyTo(expectedBytes.AsSpan(5));
            // Port
            BitConverter.GetBytes(port).Reverse().ToArray().CopyTo(expectedBytes.AsSpan(expectedBytes.Length - 2));

            CommandRequest request = new(new DnsEndPoint(domainName, port))
            {
                Version = version,
                Command = command,
            };

            Assert.Equal(expectedBytes, request.Bytes);
            // FIXME: Throwing exceptions from properties results in failure
            // NOTE: Consider returning a default value instead of throwing an exception for properties
            // Assert.Equivalent(new CommandRequest(expectedBytes), request);
        }

        [Theory]
        [InlineData(0x22, ProxyCommand.Connect, "10.0.0.122", 1337, true)]
        [InlineData(0x22, ProxyCommand.Connect, "2001:db8::aaaa:c0c0:4444", 1337, false)]
        [InlineData(ProxyConsts.Version, ProxyCommand.UdpAssociate, "192.168.12.23", 4242, true)]
        [InlineData(ProxyConsts.Version, ProxyCommand.Bind, "2001:db8::2222:cbba:1234", 4212, false)]
        [InlineData(ProxyConsts.Version, (ProxyCommand)byte.MaxValue, "0.0.0.0", 1042, true)]
        [InlineData(ProxyConsts.Version, (ProxyCommand)byte.MaxValue, "2001:db8::ffff:0001", 2, false)]
        public void Bytes_MatchStructure_IPEndPoint(byte version, ProxyCommand command, string ipAddress, ushort port, bool isIpv4)
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            byte[] expectedBytes = [
                // Version
                version,
                // Command
                (byte)command,
                // Reserved
                0x0,
                // Address type
                isIpv4 ? (byte)0x01 : (byte)0x04,
                // Address
                // Port
            ];
            Array.Resize(ref expectedBytes, expectedBytes.Length + (isIpv4 ? 4 : 16) + 2);
            // Address
            bool addressWritten = address.TryWriteBytes(expectedBytes.AsSpan(4), out _);
            Assert.True(addressWritten);
            // Port
            BitConverter.GetBytes(port).Reverse().ToArray().CopyTo(expectedBytes.AsSpan(expectedBytes.Length - 2));

            CommandRequest request = new(new IPEndPoint(address, port))
            {
                Version = version,
                Command = command,
            };

            Assert.Equal(expectedBytes, request.Bytes);
            // FIXME: Throwing exceptions from properties results in failure
            // NOTE: Consider returning a default value instead of throwing an exception for properties
            // Assert.Equivalent(new CommandRequest(expectedBytes), request);
        }

        // TODO: Add unit tests for Validate() and the properties
    }
}
