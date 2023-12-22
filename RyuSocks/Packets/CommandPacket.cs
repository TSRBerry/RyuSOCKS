using System;

namespace RyuSocks.Packets
{
    public abstract class CommandPacket : EndpointPacket
    {
        public byte Version;
        // ProxyCommand or ReplyField
        public byte Reserved;
        
        public override void Verify()
        {
            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException(
                    $"Version field is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }

            if (Reserved != 0)
            {
                throw new InvalidOperationException($"{nameof(Reserved)} field is not 0: {Reserved}");
            }
        }
    }
}
