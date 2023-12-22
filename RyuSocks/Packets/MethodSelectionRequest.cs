using RyuSocks.Auth;
using System;
using System.Linq;

namespace RyuSocks.Packets
{
    public class MethodSelectionRequest : IPacket
    {
        public byte Version;
        public byte NumOfMethods;
        private AuthMethod[] _methods = new AuthMethod[0xFF];

        public AuthMethod[] Methods
        {
            get => _methods[..NumOfMethods];
            set
            {
                _methods = value;
                Array.Resize(ref _methods, 0xFF);
            }
        }

        public void FromArray(byte[] array)
        {
            Version = array[0];
            NumOfMethods = array[1];
            array[2..].CopyTo(_methods, 0);
        }

        public byte[] ToArray()
        {
            // Version + NumOfMethods + Methods
            byte[] array = new byte[1 + 1 + NumOfMethods];

            array[0] = Version;
            array[1] = NumOfMethods;
            Methods.CopyTo(array, 2);

            return array;
        }

        public void Verify()
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
