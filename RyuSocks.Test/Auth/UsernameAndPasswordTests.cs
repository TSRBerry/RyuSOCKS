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
using RyuSocks.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RyuSocks.Test.Auth
{
    [ExcludeFromCodeCoverage]
    public class UsernameAndPasswordTests
    {
        private static readonly UsernameAndPasswordResponse _expectedUsernameAndPasswordResponse = new()
        {
            Version = AuthConsts.UsernameAndPasswordVersion,
            Status = 0,
        };

        public class NoExistingData
        {
            private readonly UsernameAndPassword _usernameAndPassword = new() { Database = [], };

            private void SetUsernameAndPassword(string username, string password)
            {
                _usernameAndPassword.Username = username;
                _usernameAndPassword.Password = password;
            }

            [Theory]
            [InlineData("", "")]
            [InlineData("", "Password")]
            [InlineData("Username", "")]
            [InlineData("Username", "Password")]
            public void Authenticate_DoesNotWorkOnEmptyUserListClientSide(string username, string password)
            {
                UsernameAndPasswordRequest expectedUsernameAndPasswordRequest = new(username, password);
                UsernameAndPasswordResponse expectedUsernameAndPasswordResponse = new([AuthConsts.UsernameAndPasswordVersion, 1]);
                SetUsernameAndPassword(username, password);
                UsernameAndPassword usernameAndPassword = _usernameAndPassword;
                usernameAndPassword.IsClient = true;
                usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> responsePacket);

                Assert.Equal(responsePacket, expectedUsernameAndPasswordRequest.Bytes);
                Assert.ThrowsAny<Exception>(() =>
                    usernameAndPassword.Authenticate(expectedUsernameAndPasswordResponse.Bytes, out _));
            }

            [Theory]
            [InlineData("", "")]
            [InlineData("", "Password")]
            [InlineData("Username", "")]
            [InlineData("Username", "Password")]
            public void Authenticate_DoesNotWorkOnEmptyUserListServerSide(string username, string password)
            {
                UsernameAndPasswordRequest expectedUsernameAndPasswordRequest = new(username, password);
                SetUsernameAndPassword(username, password);
                UsernameAndPassword usernameAndPassword = _usernameAndPassword;
                usernameAndPassword.IsClient = false;

                Assert.ThrowsAny<Exception>(() =>
                    usernameAndPassword.Authenticate(expectedUsernameAndPasswordRequest.Bytes,
                        out ReadOnlySpan<byte> _));
            }
        }

        public class EntryProvided
        {
            private const String ValidUsername = "Username";
            private const String ValidPassword = "Password";
            private readonly UsernameAndPassword _usernameAndPassword = new() { Database = [] };

            public EntryProvided()
            {
                _usernameAndPassword.Database.Add(ValidUsername, ValidPassword);
                _usernameAndPassword.IsClient = true;
            }

            private void SetUsernameAndPassword(string username, string password)
            {
                _usernameAndPassword.Username = username;
                _usernameAndPassword.Password = password;
            }

            [Theory]
            [InlineData(ValidUsername, ValidPassword, true)]
            [InlineData(ValidUsername, "RandomPassword", false)]
            [InlineData("RandomUsername", ValidPassword, false)]
            [InlineData("RandomUsername", "RandomPassword", false)]
            public void Authenticate_WorksOnRegisteredUserAuthenticationClientSide(string username, string password, bool registered)
            {
                UsernameAndPasswordRequest expectedUsernameAndPasswordRequest = new(username, password);
                UsernameAndPasswordResponse expectedUsernameAndPasswordResponse = registered ?
                                    new UsernameAndPasswordResponse([AuthConsts.UsernameAndPasswordVersion, 0]) : new UsernameAndPasswordResponse([AuthConsts.UsernameAndPasswordVersion, 1]);
                SetUsernameAndPassword(username, password);
                UsernameAndPassword usernameAndPassword = _usernameAndPassword;
                usernameAndPassword.IsClient = true;
                usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> responsePacket);
                Assert.Equal(responsePacket, expectedUsernameAndPasswordRequest.Bytes);
                if (registered)
                {
                    Assert.True(usernameAndPassword.Authenticate(expectedUsernameAndPasswordResponse.Bytes, out _));
                }
                else
                {
                    Assert.ThrowsAny<Exception>(() =>
                        usernameAndPassword.Authenticate(expectedUsernameAndPasswordResponse.Bytes, out _));
                }
            }

            [Theory]
            [InlineData(ValidUsername, ValidPassword, true)]
            [InlineData(ValidUsername, "RandomPassword", false)]
            [InlineData("RandomUsername", ValidPassword, false)]
            [InlineData("RandomUsername", "RandomPassword", false)]
            public void Authenticate_WorksOnRegisteredUserAuthenticationServerSide(string username, string password,
                bool registered)
            {
                SetUsernameAndPassword(username, password);
                UsernameAndPassword usernameAndPassword = _usernameAndPassword;
                usernameAndPassword.Authenticate(null, out ReadOnlySpan<byte> outgoingPacket);
                UsernameAndPasswordRequest input = new(outgoingPacket.ToArray());
                input.Validate();

                if (!registered)
                {
                    // Since a ByRef variable (of type ReadOnlySpan) is not accepted by a lambda expression, it is converted to a byte array
                    byte[] outgoingPacketByte = outgoingPacket.ToArray();
                    Assert.ThrowsAny<Exception>(() => usernameAndPassword.Authenticate(outgoingPacketByte, out _));
                }
                else
                {
                    usernameAndPassword.IsClient = false;
                    usernameAndPassword.Authenticate(outgoingPacket, out ReadOnlySpan<byte> responsePacket);
                    Assert.Equal(_expectedUsernameAndPasswordResponse.AsSpan(), responsePacket);
                }
            }
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
        public void Wrap_DoesNotModifyPaket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            UsernameAndPassword auth = new();

            int wrappedPacketLength = auth.Wrap(packet, packetLength, null);

            Assert.Equal(0, auth.WrapperLength);
            Assert.Equal(packetLength, wrappedPacketLength);
            Assert.Equal(originalPacket, packet);
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC, 0xBB })]
        public void Unwrap_DoesNotModifyPaket(byte[] packet)
        {
            byte[] originalPacket = (byte[])packet.Clone();
            int packetLength = packet.Length;
            UsernameAndPassword auth = new();

            int unwrappedPacketLength = auth.Unwrap(packet, packetLength, out ProxyEndpoint remoteEndpoint);

            Assert.Equal(0, auth.WrapperLength);
            Assert.Null(remoteEndpoint);
            Assert.Equal(packetLength, unwrappedPacketLength);
            Assert.Equal(originalPacket, packet);
        }
    }
}
