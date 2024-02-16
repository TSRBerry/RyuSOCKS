using System;

namespace RyuSocks.Commands.Client
{
    public class ConnectCommand : IClientCommand
    {
        public static ProxyCommand Id => ProxyCommand.Connect;

        private readonly SocksClient _parent;

        public ConnectCommand(SocksClient parent)
        {
            _parent = parent;
        }

        public void SendAsync(Span<byte> buffer)
        {
            _parent.SendAsync(buffer);
        }

        public byte[] Receive(byte[] buffer, long offset, long size)
        {
            return buffer;
        }
    }
}
