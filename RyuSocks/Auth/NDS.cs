using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Novell Directory Service (NDS) authentication
    /// </summary>
    [AuthMethodImpl(0x07)]
    public class NDS : IProxyAuth
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