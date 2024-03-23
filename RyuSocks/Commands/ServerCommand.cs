using System;
using System.Net;

namespace RyuSocks.Commands
{
    public abstract class ServerCommand : Command
    {
        protected static readonly IPEndPoint NullEndPoint = new(0, 0);

        protected readonly SocksSession Session;
        protected readonly IPEndPoint BoundEndpoint;

        protected ServerCommand(SocksSession session, IPEndPoint boundEndpoint, EndPoint destination) : base(destination)
        {
            Session = session;
            BoundEndpoint = boundEndpoint;
        }

        /// <summary>
        /// Handle data received from the remote.
        /// </summary>
        /// <param name="buffer">The data received from the remote.</param>
        public abstract void OnReceived(ReadOnlySpan<byte> buffer);
    }
}
