using RyuSocks.Types;
using System;
using System.Net;

namespace RyuSocks.Commands
{
    public abstract class Command : IWrapper
    {
        protected readonly ProxyEndpoint ProxyEndpoint;

        protected Command(ProxyEndpoint proxyEndpoint)
        {
            ProxyEndpoint = proxyEndpoint;
        }

        public virtual ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet, ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            wrapperLength = 0;
            return packet;
        }

        public virtual Span<byte> Unwrap(Span<byte> packet, out ProxyEndpoint remoteEndpoint, out int wrapperLength)
        {
            wrapperLength = 0;
            remoteEndpoint = ProxyEndpoint;
            return packet;
        }
    }
}
