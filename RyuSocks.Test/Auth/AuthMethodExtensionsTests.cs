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
using RyuSocks.Test.Utils;
using RyuSocks.Types;
using System;
using Xunit;


namespace RyuSocks.Test.Auth
{
    public class AuthMethodExtensionsTests
    {
        public static readonly TheoryData<AuthMethod, IProxyAuth> AuthImplObjects = new()
        {
            { AuthMethod.NoAuth, new NoAuth() },
            { AuthMethod.GssApi, new GssApi() },
            { AuthMethod.UsernameAndPassword, new UsernameAndPassword() },
            { AuthMethod.CHAP, new CHAP() },
            { AuthMethod.CRAM, new CRAM() },
            { AuthMethod.SSL, new SSL() },
            { AuthMethod.NDS, new NDS() },
            { AuthMethod.MAF, new MAF() },
            { AuthMethod.JSONParameterBlock, new JSONParameterBlock() },
        };

        [Theory]
        [EnumData<AuthMethod>]
        public void GetAuth_ReturnsInstance(AuthMethod authMethod)
        {
            switch (authMethod)
            {
                case AuthMethod.NoAuth:
                    Assert.IsType<NoAuth>(authMethod.GetAuth());
                    break;
                case AuthMethod.GssApi:
                    Assert.IsType<GssApi>(authMethod.GetAuth());
                    break;
                case AuthMethod.UsernameAndPassword:
                    Assert.IsType<UsernameAndPassword>(authMethod.GetAuth());
                    break;
                case AuthMethod.CHAP:
                    Assert.IsType<CHAP>(authMethod.GetAuth());
                    break;
                case AuthMethod.CRAM:
                    Assert.IsType<CRAM>(authMethod.GetAuth());
                    break;
                case AuthMethod.SSL:
                    Assert.IsType<SSL>(authMethod.GetAuth());
                    break;
                case AuthMethod.NDS:
                    Assert.IsType<NDS>(authMethod.GetAuth());
                    break;
                case AuthMethod.MAF:
                    Assert.IsType<MAF>(authMethod.GetAuth());
                    break;
                case AuthMethod.JSONParameterBlock:
                    Assert.IsType<JSONParameterBlock>(authMethod.GetAuth());
                    break;
                case AuthMethod.NoAcceptableMethods:
                    Assert.Throws<ArgumentException>(() => authMethod.GetAuth());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(authMethod), authMethod, "Missing test case.");
            }
        }

        [Theory]
#pragma warning disable xUnit1045 // Avoid using TheoryData type arguments that might not be serializable
        [MemberData(nameof(AuthImplObjects))]
#pragma warning restore xUnit1045
        public void GetAuth_ReturnsEnumValue(AuthMethod authMethod, IProxyAuth authImpl)
        {
            Assert.Equal(authMethod, authImpl.GetAuth());
        }

        [Fact]
        public void GetAuth_ThrowsOnUnknownImpl()
        {
            Assert.Throws<ArgumentException>(() => new UnknownAuth().GetAuth());
        }
    }

    internal class UnknownAuth : IProxyAuth
    {
        public int WrapperLength => throw new NotImplementedException();

        public int Wrap(Span<byte> packet, int packetLength, ProxyEndpoint remoteEndpoint)
        {
            throw new NotImplementedException();
        }

        public int Unwrap(Span<byte> packet, int packetLength, out ProxyEndpoint remoteEndpoint)
        {
            throw new NotImplementedException();
        }

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            throw new NotImplementedException();
        }
    }
}
