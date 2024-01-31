using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Challenge-Handshake Authentication Protocol
    /// (draft-ietf-aft-socks-chap-01)
    /// </summary>
    [AuthMethodImpl(0x03)]
    public class CHAP : IProxyAuth
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
