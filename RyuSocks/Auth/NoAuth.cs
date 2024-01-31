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

        public bool Authenticate(ReadOnlySpan<byte> incomingPacket, out ReadOnlySpan<byte> outgoingPacket)
        {
            // Nothing to do here.
            outgoingPacket = null;
            return true;
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
