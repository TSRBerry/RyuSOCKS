using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Challenge-Response Authentication Method
    /// (draft-ietf-aft-socks-cram-00)
    /// </summary>
    [AuthMethodImpl(0x05)]
    public class CRAM : IProxyAuth
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
