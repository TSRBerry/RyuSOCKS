using RyuSocks.Auth;
using System;

namespace RyuSocks.Packets
{
    public struct MethodSelectionResponse : IPacket
    {
        public byte Version;
        public AuthMethod Method;

        public void FromArray(byte[] array)
        {
            Version = array[0];
            Method = (AuthMethod)array[1];
        }

        public byte[] ToArray()
        {
            return [Version, (byte)Method];
        }

        public readonly void Verify()
        {
            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException($"Version field is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }
        }
    }
}
