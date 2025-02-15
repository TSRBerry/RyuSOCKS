using RyuSocks.Auth;
using RyuSocks.Auth.Packets;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RyuSocks.Test.Auth
{
    [ExcludeFromCodeCoverage]
    public class UsernameAndPasswordResponseTest
    {
        [Theory]
        [InlineData(0x03, false)]
        [InlineData(AuthConsts.UsernameAndPasswordVersion, true)]
        public void Validate_ThrowsOnWrongVersion(byte version, bool hasRightVersion)
        {
            UsernameAndPasswordResponse expectedUsernameAndPasswordResponse = new([version, 0]);

            if (hasRightVersion)
            {
                expectedUsernameAndPasswordResponse.Validate();
            }
            else
            {
                Assert.ThrowsAny<Exception>(() => expectedUsernameAndPasswordResponse.Validate());
            }
        }
    }
}
