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
using RyuSocks.Test.Utils;
using RyuSocks.Utils;
using System;
using System.Linq;
using Xunit;

namespace RyuSocks.Test.Packets
{
    public class MethodSelectionRequestTests
    {
        [Theory]
        [InlineData(new[] { AuthMethod.NoAuth })]
        [RangeData<byte>(byte.MinValue, byte.MaxValue)]
        [RangeData<byte>(byte.MinValue, byte.MaxValue, [0x0, 0xFF, 0xAA, 0xBB])]
        [InlineData(new AuthMethod[] { })]
        public void Bytes_Size(AuthMethod[] methods)
        {
            // Version: 1 byte
            // Number of methods: 1 byte
            // Methods: 1 - 255 bytes
            // Total: 3 - 257 bytes

            if (methods.Length > byte.MaxValue)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new MethodSelectionRequest(methods));
                return;
            }

            MethodSelectionRequest request = new(methods);

            Assert.Equal(2 + methods.Length, request.Bytes.Length);
        }

        [Theory]
        [InlineData(0x22, new[] { AuthMethod.NoAuth, AuthMethod.UsernameAndPassword })]
        [InlineData(ProxyConsts.Version, new[] { AuthMethod.NoAcceptableMethods })]
        public void Bytes_MatchStructure(byte version, AuthMethod[] methods)
        {
            byte[] expectedBytes = [
                // Version
                version,
                // Number of methods
                (byte)methods.Length,
                // Methods
            ];
            Array.Resize(ref expectedBytes, expectedBytes.Length + methods.Length);
            methods.Cast<byte>().ToArray().CopyTo(expectedBytes.AsSpan(2));

            MethodSelectionRequest request = new(methods)
            {
                Version = version,
            };

            Assert.Equal(expectedBytes, request.Bytes);
            Assert.Equivalent(new MethodSelectionRequest(expectedBytes), request);
        }

        [Theory]
        [InlineData(0, new[] { AuthMethod.NoAcceptableMethods })]
        [InlineData(1, new[] { AuthMethod.NoAuth })]
        [InlineData(ProxyConsts.Version, new[] { AuthMethod.NoAuth, AuthMethod.NoAcceptableMethods })]
        public void Properties_ValuesMatch(byte version, AuthMethod[] methods)
        {
            MethodSelectionRequest request = new(methods)
            {
                Version = version,
            };

            Assert.Equal(version, request.Version);
            Assert.Equal(methods.Length, request.NumOfMethods);
            Assert.Equal(methods, request.Methods);
        }

        [Theory]
        [InlineData(0, new[] { AuthMethod.NoAuth }, false)]
        [InlineData(0, new[] { AuthMethod.NoAcceptableMethods }, false)]
        [InlineData(1, new[] { AuthMethod.UsernameAndPassword }, false)]
        [InlineData(ProxyConsts.Version, new[] { AuthMethod.NoAuth, AuthMethod.NoAcceptableMethods }, false)]
        [InlineData(ProxyConsts.Version, new[] { AuthMethod.NoAcceptableMethods }, false)]
        [InlineData(ProxyConsts.Version, new[] { AuthMethod.NoAuth }, true)]
        public void Validate_ThrowsOnInvalidValue(byte version, AuthMethod[] methods, bool isValidInput)
        {
            MethodSelectionRequest request = new(methods)
            {
                Version = version,
            };

            if (!isValidInput)
            {
                Assert.Throws<InvalidOperationException>(() => request.Validate());
                return;
            }

            request.Validate();
        }
    }
}
