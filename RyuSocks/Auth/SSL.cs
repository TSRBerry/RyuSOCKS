using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// Secure Sockets Layer
    /// (draft-ietf-aft-socks-ssl-00)
    /// </summary>
    [AuthMethodImpl(0x06)]
    public class SSL : IProxyAuth
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
