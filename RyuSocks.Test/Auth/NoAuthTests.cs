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
using RyuSocks.Auth.Extensions;
using RyuSocks.Types;
using System;
using Xunit;

namespace RyuSocks.Test.Auth
{
    public class NoAuthTests
    {
        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
        public void Wrap_DoesNotModifyPaket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            NoAuth noAuth = new();

            byte[] wrappedPacket = noAuth.Wrap(packet, null, out int wrapperLength).ToArray();

            Assert.Equal(0, wrapperLength);
            Assert.Equal(packetLength, packet.Length);
            Assert.Equal(originalPacket, packet);
            Assert.Equal(originalPacket, wrappedPacket);
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
        public void Unwrap_DoesNotModifyPacket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            NoAuth noAuth = new();

            byte[] wrappedPacket = noAuth.Unwrap(packet, out ProxyEndpoint remoteEndpoint, out int wrapperLength).ToArray();

            Assert.Equal(0, wrapperLength);
            Assert.Null(remoteEndpoint);
            Assert.Equal(packetLength, packet.Length);
            Assert.Equal(originalPacket, packet);
            Assert.Equal(originalPacket, wrappedPacket);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
        public void Authenticate_DoesNothing(byte[] incomingPacket)
        {
            NoAuth noAuth = new();

            bool authDone = noAuth.Authenticate(incomingPacket, out ReadOnlySpan<byte> outgoingPacket);

            Assert.True(authDone);
            // FIXME: Unable to use Assert.Null() for some reason.
            if (outgoingPacket != null)
            {
                Assert.Fail($"{nameof(outgoingPacket)} is not null.");
            }
        }

        [Fact]
        public void GetAuth_ReturnsNoAuth()
        {
            NoAuth noAuth = new();

            AuthMethod authMethod = noAuth.GetAuth();

            Assert.Equal(AuthMethod.NoAuth, authMethod);
        }
    }
}
