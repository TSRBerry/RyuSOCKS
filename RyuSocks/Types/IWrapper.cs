using System;

namespace RyuSocks.Types
{
    public interface IWrapper
    {
        /// <summary>
        /// Wrap the packet as required.
        /// </summary>
        /// <param name="packet">The packet to wrap.</param>
        /// <param name="remoteEndpoint">The destination endpoint of this packet.</param>
        /// <param name="wrapperLength">The amount of bytes added by the wrapper.</param>
        /// <returns>The wrapped packet.</returns>
        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet, ProxyEndpoint remoteEndpoint, out int wrapperLength);

        /// <summary>
        /// Unwrap the packet and perform the checks as required.
        /// </summary>
        /// <param name="packet">The packet to unwrap.</param>
        /// <param name="remoteEndpoint">The source endpoint of this packet.</param>
        /// <param name="wrapperLength">The amount of bytes removed by the wrapper.</param>
        /// <returns>The unwrapped packet.</returns>
        public Span<byte> Unwrap(Span<byte> packet, out ProxyEndpoint remoteEndpoint, out int wrapperLength);
    }
}
