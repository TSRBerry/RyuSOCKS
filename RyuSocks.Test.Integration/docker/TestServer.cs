// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Net;
using RyuSocks;
using RyuSocks.Auth;
using RyuSocks.Commands;
using RyuSocks.Types;

namespace TestProject
{
    public class TestSession : SocksSession
    {
        private EndPoint _remoteEndpoint;
        public TestSession(NetCoreServer.TcpServer server) : base(server) { }

        private void LogMsg(string msg)
        {
            Console.WriteLine($"<SocksSession ({_remoteEndpoint})> {msg}");
        }

        protected override void OnConnected()
        {
            _remoteEndpoint ??= Socket.RemoteEndPoint;
            LogMsg("Connected!");
            base.OnConnected();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _remoteEndpoint ??= Socket.RemoteEndPoint;
            LogMsg($"Received packet of size: {size}");
            base.OnReceived(buffer, offset, size);
            LogMsg($"IsAuthenticated: {IsAuthenticated}");
            LogMsg($"Command: {Command}");
        }

    }

    public class TestSocksServer : SocksServer
    {
        public TestSocksServer() : base(IPAddress.Any)
        {

        }

        protected override NetCoreServer.TcpSession CreateSession()
        {
            return new TestSession(this);
        }
    }

    public static class Program
    {
        private static readonly SocksServer _server = new TestSocksServer()
        {
            AcceptableAuthMethods = new HashSet<AuthMethod>() { AuthMethod.NoAuth },
            OfferedCommands = new HashSet<ProxyCommand>() { ProxyCommand.Connect, ProxyCommand.Bind, ProxyCommand.UdpAssociate },
        };

        public static int Main(string[] args)
        {
            Console.WriteLine("Starting SOCKS5 proxy server...");

            if (!_server.Start())
            {
                Console.WriteLine("Failed to start SOCKS5 proxy server.");
                return 1;
            }

            Console.WriteLine($"SOCKS5 proxy server started: {_server.Endpoint}");

            while (_server.IsStarted)
            {

            }

            Console.WriteLine($"SOCKS5 proxy server stopped.");

            _server.Dispose();
            return 0;
        }
    }
}
