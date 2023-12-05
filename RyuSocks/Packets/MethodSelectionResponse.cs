using RyuSocks.Auth;

namespace RyuSocks.Packets
{
    public struct MethodSelectionResponse
    {
        public byte Version;
        public AuthMethod Method;
    }
}
