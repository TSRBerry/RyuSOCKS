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
using RyuSocks.Auth.Packets;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Xunit;

namespace RyuSocks.Test.Auth
{
    public class UsernameAndPasswordTests
    {
        private static readonly UsernameAndPasswordResponse _expectedUsernameAndPasswordResponse = new()
        {
            Version = AuthConsts.UsernameAndPasswordVersion,
            Status = 0,
        };

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC }, false)]
        [InlineData(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, true)]
        [InlineData(new byte[] { 0x01 }, false)]
        [InlineData(new byte[] { }, false)]
        [InlineData(new byte[] { 0x74 }, false)]
        public void Authenticate_ThrowsOnWrongVersion(byte[] incomingPacket, bool isValidInput)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = [],
                IsClient = false
            };

            if (!isValidInput)
            {
                Assert.Throws<InvalidOperationException>(() => usernameAndPassword.Authenticate(incomingPacket, out _));
            }
            else
            {
                Assert.Throws<AuthenticationException>(() => usernameAndPassword.Authenticate(incomingPacket, out _));
            }
        }

        [Theory]
        [InlineData("Username", "Password", true)]
        [InlineData("InvalidUsername", "InvalidPassword", false)]
        public void Authenticate_WorksOnRegisteredUserAuthenticationClientSide(string username, string password,
            bool acceptable)
        {
            UsernameAndPasswordRequest expectedUsernameAndPasswordRequest =
                new UsernameAndPasswordRequest(username, password);
            UsernameAndPasswordResponse expectedUsernameAndPasswordResponse = acceptable ? new UsernameAndPasswordResponse([Constants.UsernameAndPasswordVersion, 0]) : new UsernameAndPasswordResponse([Constants.UsernameAndPasswordVersion, 1]);
            byte[] expectedUsernameAndPasswordResponseBytes = expectedUsernameAndPasswordResponse.Bytes;
            byte[] expectedUsernameAndPasswordRequestBytes = expectedUsernameAndPasswordRequest.Bytes;
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = new Dictionary<string, string>{{"Username", "Password"}},
                Username = username,
                Password = password,
                IsClient = true
            };

            usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> responsePacket);
            Assert.Equal(responsePacket, expectedUsernameAndPasswordRequestBytes);
            if (acceptable)
            {
                Assert.True(usernameAndPassword.Authenticate(expectedUsernameAndPasswordResponseBytes, out _));
            }
            else
            {
                 Assert.Throws<AuthenticationException>(() => usernameAndPassword.Authenticate(expectedUsernameAndPasswordResponseBytes, out _));
            }
        }

        [Theory]
        [InlineData("RegisteredUsername", "RegisteredPassword", 0)]
        [InlineData("NotRegisteredUsername", "NotRegisteredPassword", 1)]
        [InlineData("RegisteredUsername", "WrongPassword", 2)]
        public void Authenticate_WorksOnRegisteredUserAuthenticationServerSide(string username, string password, int inputTypes)
        {
            UsernameAndPassword usernameAndPassword = new()
            {
                Database = inputTypes switch
                {
                    0 => new Dictionary<string, string> { { username, password } },
                    2 => new Dictionary<string, string> { { username, "RegisteredPassword" } },
                    _ => []
                },
                Username = username,
                Password = password
            };

            usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> outgoingPacket);

            if (inputTypes is 1 or 2)
            {
                // Since a ByRef variable (of type ReadOnlySpan) is not accepted by a lambda expression, it is converted to a byte array
                byte[] outgoingPacketByte = outgoingPacket.ToArray();
                Assert.Throws<AuthenticationException>(() => usernameAndPassword.Authenticate(outgoingPacketByte, out _));
            }
            else
            {
                usernameAndPassword.IsClient = false;
                usernameAndPassword.Authenticate(outgoingPacket, out ReadOnlySpan<byte> responsePacket);
                Assert.Equal(_expectedUsernameAndPasswordResponse.AsSpan(), responsePacket);
            }
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
        public void Wrap_DoesNotModifyPaket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            NoAuth noAuth = new();

            byte[] wrappedPacket = noAuth.Wrap(packet, null, out _).ToArray();

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

            byte[] wrappedPacket = noAuth.Unwrap(packet, out _, out _).ToArray();

            Assert.Equal(packetLength, packet.Length);
            Assert.Equal(originalPacket, packet);
            Assert.Equal(originalPacket, wrappedPacket);
        }
    }
}
