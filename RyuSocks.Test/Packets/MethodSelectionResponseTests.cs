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

using RyuSocks.Auth;
using RyuSocks.Packets;
using RyuSocks.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RyuSocks.Test.Packets
{
    [ExcludeFromCodeCoverage]
    public class MethodSelectionResponseTests
    {
        [Fact]
        public void Bytes_Size()
        {
            // Version: 1 byte
            // Method: 1 byte
            // Total: 2 bytes

            var response = new MethodSelectionResponse(AuthMethod.NoAuth);

            Assert.Equal(2, response.Bytes.Length);
        }

        [Theory]
        [InlineData(0x22, AuthMethod.UsernameAndPassword)]
        public void Bytes_MatchStructure(byte version, AuthMethod method)
        {
            byte[] expectedBytes = [
                // Version
                version,
                // Method
                (byte)method,
            ];

            MethodSelectionResponse response = new(method)
            {
                Version = version,
            };

            Assert.Equal(expectedBytes, response.Bytes);
            Assert.Equivalent(new MethodSelectionResponse(expectedBytes), response, true);
        }

        [Theory]
        [InlineData(0x22, AuthMethod.UsernameAndPassword)]
        public void Properties_ValuesMatch(byte version, AuthMethod method)
        {
            MethodSelectionResponse response = new(method)
            {
                Version = version,
            };

            Assert.Equal(version, response.Version);
            Assert.Equal(method, response.Method);
        }

        [Theory]
        [InlineData(0, AuthMethod.NoAcceptableMethods, false)]
        [InlineData(0, AuthMethod.NoAuth, false)]
        [InlineData(1, AuthMethod.NoAcceptableMethods, false)]
        [InlineData(1, AuthMethod.NoAuth, false)]
        [InlineData(ProxyConsts.Version, AuthMethod.NoAuth, true)]
        [InlineData(ProxyConsts.Version, AuthMethod.NoAcceptableMethods, true)]
        public void Validate_ThrowsOnInvalidValue(byte version, AuthMethod method, bool isValidInput)
        {
            MethodSelectionResponse response = new(method)
            {
                Version = version,
            };

            if (!isValidInput)
            {
                Assert.ThrowsAny<Exception>(() => response.Validate());
                return;
            }

            response.Validate();
        }

        [Theory]
        // 0 bytes
        [InlineData(new byte[] { }, false)]
        // 1 byte
        [InlineData(new byte[] { ProxyConsts.Version }, false)]
        // 2 bytes
        [InlineData(new byte[] { ProxyConsts.Version, 0x0 }, true)]
        // 3 bytes
        [InlineData(new byte[] { ProxyConsts.Version, 0x0, 0x0 }, false)]
        public void Validate_ThrowsOnInvalidLength(byte[] packetBytes, bool isValidInput)
        {
            MethodSelectionResponse response = new(packetBytes);

            if (!isValidInput)
            {
                Assert.ThrowsAny<Exception>(() => response.Validate());
                return;
            }

            response.Validate();
        }
    }
}
