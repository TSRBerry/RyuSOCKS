using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// No authentication required
    /// (RFC1928)
    /// </summary>
    [AuthMethodImpl(0x00)]
    public class NoAuth : IProxyAuth
    {
        public int WrapperLength => 0;

        public void Authenticate()
        {
            // Nothing to do here.
        }

        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet)
        {
            return packet;
        }

        public ReadOnlySpan<byte> Unwrap(ReadOnlySpan<byte> packet)
        {
            return packet;
        }
    }
}
