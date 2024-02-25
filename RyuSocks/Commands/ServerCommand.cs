using System;
using System.Net;

namespace RyuSocks.Commands
{
    public abstract class ServerCommand : Command
    {
        protected readonly SocksSession Session;

        protected ServerCommand(SocksSession session, EndPoint destination) : base(destination)
        {
            Session = session;
        }

        /// <summary>
        /// Handle data received from the remote.
        /// </summary>
        /// <param name="buffer">The data received from the remote.</param>
        public abstract void OnReceived(ReadOnlySpan<byte> buffer);
    }
}
