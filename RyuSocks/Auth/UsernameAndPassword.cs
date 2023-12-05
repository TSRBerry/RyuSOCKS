using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Username/Password Authentication
    /// (RFC1929)
    /// </summary>
    [AuthMethodImpl(0x02)]
    public class UsernameAndPassword : IProxyAuth
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
