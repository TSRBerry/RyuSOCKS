using RyuSocks.Commands.Client;
using System;
using System.Collections.Generic;
using System.Net;

namespace RyuSocks
{
    public class SocksClient : TcpClient
    {
        public readonly Dictionary<byte, Func<SocksClient, IClientCommand>> Commands = new()
        {
            { (byte)ConnectCommand.Id, (client) => new ConnectCommand(client) },
        };

        public SocksClient(IPAddress address, int port) : base(address, port)
        {
        }

        public SocksClient(string address, int port) : base(address, port)
        {
        }

        public SocksClient(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public SocksClient(IPEndPoint endpoint) : base(endpoint)
        {
        }
    }
}
