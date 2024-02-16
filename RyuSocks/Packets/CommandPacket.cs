using System;

namespace RyuSocks.Packets
{
    public abstract class CommandPacket : EndpointPacket
    {
        public byte Version
        {
            get
            {
                return Bytes[0];
            }
            set
            {
                Bytes[0] = value;
            }
        }

        // ProxyCommand or ReplyField

        public byte Reserved
        {
            get
            {
                return Bytes[2];
            }
            set
            {
                Bytes[2] = value;
            }
        }

        // AddressType

        // Address

        // Port

        public override void Validate()
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
