using RyuSocks.Auth;
using System;

namespace RyuSocks.Server
{
    [AuthMethodImpl(0x71)]
    public class TestMe : IProxyAuth
    {
        public int WrapperLength { get; }
        public void Authenticate()
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> Unwrap(ReadOnlySpan<byte> packet)
        {
            throw new NotImplementedException();
        }
    }
}
