using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Multi-Authentication Framework
    /// (draft-ietf-aft-socks-maf-01)
    /// </summary>
    [AuthMethodImpl(0x08)]
    public class MAF : IProxyAuth
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
