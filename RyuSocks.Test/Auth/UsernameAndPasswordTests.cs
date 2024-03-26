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
using RyuSocks.Packets.Auth.UsernameAndPassword;
using System;
using System.Security.Authentication;
using Xunit;

namespace RyuSocks.Test.Auth
{
    public class UsernameAndPasswordTests
    {
        [Theory]
        [InlineData(new byte[] {0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB})]
        public void Authenticate_ServerRightResponses(byte[] incomingPacket)
        {
            UsernameAndPassword usernameAndPassword = new ();
            usernameAndPassword.IsClient = false;
            usernameAndPassword.Authenticate(incomingPacket, out ReadOnlySpan<byte> outgoingPacket);
            if(outgoingPacket == null)
            {
                Assert.Fail($"{nameof(outgoingPacket)} is null.");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[] {0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB})]
        public void Authenticate_ServerHandlingInvalidIncomingPacket(byte[] incomingPacket)
        {
            UsernameAndPassword usernameAndPassword = new ();
            usernameAndPassword.IsClient = false;
            Assert.Throws<AuthenticationException>(() => usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> _));

        }



        [Theory]
        [InlineData(new byte[] {0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB})]
        public void Wrap_DoesNotModifyPaket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            NoAuth noAuth = new();

            byte[] wrappedPacket = noAuth.Wrap(packet).ToArray();

            Assert.Equal(packetLength, packet.Length);
            Assert.Equal(originalPacket, packet);
            Assert.Equal(originalPacket, wrappedPacket);
        }

        [Theory]
        [InlineData(new byte[] {0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB})]
        public void Unwrap_DoesNotModifyPacket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            NoAuth noAuth = new();

            byte[] wrappedPacket = noAuth.Unwrap(packet).ToArray();

            Assert.Equal(packetLength, packet.Length);
            Assert.Equal(originalPacket, packet);
            Assert.Equal(originalPacket, wrappedPacket);
        }
    }
}
