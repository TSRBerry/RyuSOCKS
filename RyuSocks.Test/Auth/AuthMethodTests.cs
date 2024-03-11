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
using System;
using Xunit;


namespace RyuSocks.Test.Auth
{
    public class AuthMethodTests
    {
        [Theory]
        [EnumData<AuthMethod>]
        public void GetAuth_ReturnValue(AuthMethod authMethod)
        {
            switch (authMethod)
            {
                case AuthMethod.NoAuth:
                    Assert.IsType<NoAuth>(authMethod.GetAuth());
                    break;
                case AuthMethod.GSSAPI:
                    Assert.IsType<GSSAPI>(authMethod.GetAuth());
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
            }
        }
    }
}
