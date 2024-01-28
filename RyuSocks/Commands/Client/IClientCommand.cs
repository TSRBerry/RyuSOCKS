using System;

namespace RyuSocks.Commands.Client
{
    public interface IClientCommand : ICommand
    {
        public byte[] Receive(byte[] buffer, long offset, long size);
        public void SendAsync(Span<byte> buffer);
    }
}
