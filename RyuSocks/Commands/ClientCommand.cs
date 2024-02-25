using System.Net;

namespace RyuSocks.Commands
{
    public abstract class ClientCommand : Command
    {
        protected readonly SocksClient Client;

        protected ClientCommand(SocksClient client, EndPoint destination) : base(destination)
        {
            Client = client;
        }
    }
}
