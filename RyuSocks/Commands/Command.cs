using System;
using System.Net;

namespace RyuSocks.Commands
{
    public abstract class Command
    {
        protected readonly EndPoint Destination;

        protected Command(EndPoint destination)
        {
            Destination = destination;
        }

        /// <summary>
        /// Wrap the packet as required for the current command.
        /// </summary>
        /// <param name="packet">The packet to wrap.</param>
        /// <returns>The wrapped packet.</returns>
        public virtual ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet)
        {
            return packet;
        }

        /// <summary>
        /// Unwrap the packet as required for the current command.
        /// </summary>
        /// <param name="packet">The packet to unwrap.</param>
        /// <returns>The unwrapped packet.</returns>
        public virtual ReadOnlySpan<byte> Unwrap(ReadOnlySpan<byte> packet)
        {
            return packet;
        }
    }
}
