using RyuSocks.Auth.Packets;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RyuSocks.Test.Auth
{
    [ExcludeFromCodeCoverage]
    public class UsernameAndPasswordRequestTests
    {
        private const string LongWord = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC }, true)]
        [InlineData(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, true)]
        [InlineData(new byte[] { 0x01 }, false)]
        [InlineData(new byte[] { }, false)]
        [InlineData(new byte[] { 0x74 }, false)]
        public void Constructor_ThrowsOnWrongLength(byte[] incomingPacket, bool hasValidLength)
        {
            if (!hasValidLength)
            {
                Assert.ThrowsAny<Exception>(() => new UsernameAndPasswordRequest(incomingPacket));
            }
            else
            {
                UsernameAndPasswordRequest _ = new(incomingPacket);
            }
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "Password", true)]
        [InlineData("Username", "", true)]
        [InlineData("Username", "Password", false)]
        public void Validate_ThrowsOnNoUsernameOrPassword(string username, string password, bool isEmpty)
        {
            UsernameAndPasswordRequest usernameAndPasswordRequest = new(username, password);

            if (!isEmpty)
            {
                usernameAndPasswordRequest.Validate();
            }
            else
            {
                Assert.ThrowsAny<Exception>(() => usernameAndPasswordRequest.Validate());
            }
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC }, false)]
        [InlineData(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, true)]
        public void Validate_ThrowsOnWrongVersion(byte[] incomingPacket, bool hasRightVersion)
        {
            UsernameAndPasswordRequest usernameAndPasswordRequest = new(incomingPacket);

            if (hasRightVersion)
            {
                usernameAndPasswordRequest.Validate();
            }
            else
            {
                Assert.ThrowsAny<Exception>(() => usernameAndPasswordRequest.Validate());
            }
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", LongWord, false)]
        [InlineData(LongWord, "", false)]
        [InlineData(LongWord, LongWord, false)]
        public void UsernamePasswordProperties_ThrowOnInvalidLength(string username, string password, bool hasValidLength)
        {
            if (hasValidLength)
            {
                UsernameAndPasswordRequest _ = new(username, password);
            }
            else
            {
                Assert.ThrowsAny<Exception>(() => new UsernameAndPasswordRequest(username, password));
            }
        }
    }
}
