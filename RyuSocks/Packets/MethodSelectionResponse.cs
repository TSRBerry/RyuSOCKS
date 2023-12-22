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
            // Version + Method
            byte[] array = new byte[1 + 1];

            array[0] = Version;
            array[1] = (byte)Method;

            return array;
        }

        public void Verify()
        {
            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException($"Version field is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }
        }
    }
}
