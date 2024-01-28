using System;

namespace RyuSocks.Auth
{
    /// <summary>
    /// JSON Parameter Block
    /// According to: https://www.iana.org/assignments/socks-methods/socks-methods.xhtml
    /// </summary>
    [AuthMethodImpl(0x09)]
    public class JSONParameterBlock : IProxyAuth
    {
        public int WrapperLength { get; }

        public void Authenticate()
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> Wrap(ReadOnlySpan<byte> packet)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> Unwrap(ReadOnlySpan<byte> packet)
        {
            throw new NotImplementedException();
        }
    }
}
