using RyuSocks.Utils;
using System;
using System.Net;

namespace RyuSocks.Commands.Server
{
    public interface IServerCommand : ICommand, IDisposable
    {
        /// <summary>
        /// Initialize the command with the provided destination endpoint
        /// </summary>
        /// <param name="destEndpoint">The destination endpoint</param>
        /// <param name="boundEndpoint">The bound endpoint on success</param>
        /// <returns>The reply field value corresponding to the result of this operation</returns>
        public ReplyField Initialize(EndPoint destEndpoint, out EndPoint boundEndpoint);
        public void SendAsync(Span<byte> buffer);
        public void OnReceived(object sender, ReceivedEventArgs args);
        public void Disconnect();
    }
}
