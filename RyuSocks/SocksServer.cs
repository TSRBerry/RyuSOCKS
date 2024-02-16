using NetCoreServer;
using RyuSocks.Auth;
using RyuSocks.Commands.Server;
using System;
using System.Collections.Generic;
using System.Net;

namespace RyuSocks
{
    public class SocksServer : TcpServer
    {
        public readonly List<AuthMethod> AuthPreferences = [];
        public readonly Dictionary<byte, Func<SocksSession, IServerCommand>> Commands = new()
        {
            { (byte)ConnectCommand.Id, (session) => new ConnectCommand(session) },
        };

        public SocksServer(IPAddress address, int port) : base(address, port)
        {
        }

        public SocksServer(string address, int port) : base(address, port)
        {
        }

        public SocksServer(DnsEndPoint endpoint) : base(endpoint)
        {
        }

        public SocksServer(IPEndPoint endpoint) : base(endpoint)
        {
        }
    }
}
