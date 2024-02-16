using RyuSocks.Auth;
using System;

namespace RyuSocks.Packets
{
    public class MethodSelectionResponse : Packet
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

        public AuthMethod Method
        {
            get
            {
                return (AuthMethod)Bytes[1];
            }
            set
            {
                Bytes[1] = (byte)value;
            }
        }

        public override void Validate()
        {
            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException($"Version field is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }
        }
    }
}
