using RyuSocks.Auth;
using System;
using System.Linq;

namespace RyuSocks.Packets
{
    public class MethodSelectionRequest : Packet
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

        public byte NumOfMethods
        {
            get
            {
                return Bytes[1];
            }
            set
            {
                Bytes[1] = value;
            }
        }

        public AuthMethod[] Methods
        {
            get
            {
                return Bytes[2..(2 + NumOfMethods)].Cast<AuthMethod>().ToArray();

            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 0xFF);
                NumOfMethods = (byte)value.Length;
                value.Cast<byte>().ToArray().CopyTo(Bytes.AsSpan(2, value.Length));
            }
        }

        public override void Validate()
        {
            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException($"Version field is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }

            if (NumOfMethods > 1 && Methods.Contains(AuthMethod.NoAcceptableMethods))
            {
                throw new InvalidOperationException(
                    $"{AuthMethod.NoAcceptableMethods} can't be offered with other auth methods.");
            }
        }
    }
}
