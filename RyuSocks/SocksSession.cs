using NetCoreServer;
using RyuSocks.Utils;
using System;

namespace RyuSocks
{
    public class SocksSession : TcpSession
    {
        public SocksSession(TcpServer server) : base(server)
        {
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {

        }
    }
}
