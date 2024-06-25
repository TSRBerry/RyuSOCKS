using RyuSocks.Packets;
using RyuSocks.Types;
using System;
using System.Runtime.CompilerServices;

namespace RyuSocks.Commands
{
    public abstract class ClientCommand : Command
    {
        protected readonly SocksClient Client;
        public bool Accepted { get; protected set; }
        public bool Ready { get; protected set; }
        public ProxyEndpoint ServerEndpoint { get; protected set; }

        protected ClientCommand(SocksClient client, ProxyEndpoint proxyEndpoint) : base(proxyEndpoint)
        {
            Client = client;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void EnsureSuccessReply(ReplyField replyField)
        {
            if (replyField != ReplyField.Succeeded)
            {
                throw new ProxyException(replyField);
            }
        }

        /// <summary>
        /// Handle the response from the server for the command request.
        /// </summary>
        /// <param name="response">The command response received from the server.</param>
        /// <exception cref="InvalidOperationException">If another invocation of this method was not expected.</exception>
        public abstract void ProcessResponse(CommandResponse response);
    }
}
