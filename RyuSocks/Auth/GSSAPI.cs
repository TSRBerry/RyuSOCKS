using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Generic Security Service Application Program Interface
    /// (RFC1961)
    /// </summary>
    [AuthMethodImpl(0x01)]
    public class GSSAPI : IProxyAuth
    {
        public int WrapperLength { get; }

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
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
