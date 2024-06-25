using RyuSocks.Types;
using System;
using System.Net;

namespace RyuSocks.Commands
{
    public abstract class Command : IWrapper
    {
        public abstract bool HandlesCommunication { get; }
        public abstract bool UsesDatagrams { get; }

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

        public virtual int Send(ReadOnlySpan<byte> buffer)
        {
            throw new NotSupportedException("This command does not require a second connection, so this method must not be called.");
        }

        public virtual int SendTo(ReadOnlySpan<byte> buffer, EndPoint endpoint)
        {
            throw new NotSupportedException("This command does not use datagrams, so this method must not be called.");
        }

        public virtual int Receive(Span<byte> buffer)
        {
            throw new NotSupportedException("This command does not require a second connection, so this method must not be called.");
        }

        public virtual int ReceiveFrom(Span<byte> buffer, ref EndPoint endpoint)
        {
            throw new NotSupportedException("This command does not use datagrams, so this method must not be called.");
        }
    }
}
