using RyuSocks.Auth;

namespace RyuSocks.Packets
{
    public struct MethodSelectionRequest1
    {
        public byte Version;
        public byte NumOfMethods;
        public Array1<AuthMethod> Methods;
    }
}
