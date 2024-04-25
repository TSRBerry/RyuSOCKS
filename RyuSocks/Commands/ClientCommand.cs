using RyuSocks.Types;

namespace RyuSocks.Commands
{
    public abstract class ClientCommand : Command
    {
        protected readonly SocksClient Client;

        protected ClientCommand(SocksClient client, ProxyEndpoint proxyEndpoint) : base(proxyEndpoint)
        {
            Client = client;
        }
    }
}
