using RyuSocks.Types;
using System;
using System.Net;

namespace RyuSocks.Commands
{
    public abstract class ServerCommand : Command
    {
        protected readonly SocksSession Session;
        protected readonly IPEndPoint BoundEndpoint;

        protected ServerCommand(SocksSession session, IPEndPoint boundEndpoint, ProxyEndpoint proxyEndpoint) : base(proxyEndpoint)
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
