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
using RyuSocks.Packets.Auth;
using RyuSocks.Packets.Auth.UsernameAndPassword;
using RyuSocks.Test.Utils;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Xunit;

namespace RyuSocks.Test.Auth
{
    public class UsernameAndPasswordTests
    {
        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC })]
        public void Authenticate_PackageWithWrongVersion(byte[] incomingPacket)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = [],
                IsClient = false
            };
            Assert.Throws<InvalidOperationException>(() => usernameAndPassword.Authenticate(incomingPacket, out _));
        }

        [Theory]
        [UsernameAndPasswordRandomUsernameAndPasswords(1)]
        public void Authenticate_RegisteredUserAuthentication(string username, string password)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = new Dictionary<string, string> { { username, password } },
                Username = username,
                Password = password
            };
            usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> outgoingPacket);
            usernameAndPassword.IsClient = false;

            usernameAndPassword.Authenticate(outgoingPacket, out _);
        }

        [Theory]
        [UsernameAndPasswordRandomUsernameAndPasswords(1)]
        public void Authenticate_NotRegisteredUserAuthentication(string username, string password)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = [],
                Username = username,
                Password = password
            };
            usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> outgoingPacket);
            // Since a ByRef variable (of type ReadOnlySpan) is not accepted by a lambda expression, it is converted to a byte array
            byte[] outgoingPacketByte = outgoingPacket.ToArray();
            usernameAndPassword.IsClient = false;
            Assert.Throws<AuthenticationException>(() => usernameAndPassword.Authenticate(outgoingPacketByte, out _));
        }

        [Theory]
        [InlineData("RegisteredUser", "WrongPassword")]
        public void Authenticate_RegisteredUserWrongPasswordAuthentication(string username, string password)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = new Dictionary<string, string> { { username, "RightPassword" } },
                Username = username,
                Password = password
            };
            usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> outgoingPacket);
            // Since a ByRef variable (of type ReadOnlySpan) is not accepted by a lambda expression, it is converted to a byte array
            byte[] outgoingPacketByte = outgoingPacket.ToArray();
            usernameAndPassword.IsClient = false;
            Assert.Throws<AuthenticationException>(() => usernameAndPassword.Authenticate(outgoingPacketByte, out _));
        }

        [Theory]
        [InlineData("a", "b")]
        public void Authenticate_RightResponsePacket(string username, string password)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = new Dictionary<string, string> { { username, password } },
                Username = username,
                Password = password
            };
            UsernameAndPasswordResponse usernameAndPasswordResponse = new()
            {
                Version = Constants.UaPVersion,
                Status = 0,
            };
            byte[] usernameAndPasswordResponseBytes = usernameAndPasswordResponse.Bytes;
            usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> outgoingPacket);
            usernameAndPassword.IsClient = false;
            usernameAndPassword.Authenticate(outgoingPacket, out ReadOnlySpan<byte> responsePacket);

            byte[] responsePacketBytes = responsePacket.ToArray();
            Assert.Equal(usernameAndPasswordResponseBytes, responsePacketBytes);
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
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
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
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
