namespace RyuSocks.Packets
{
    public interface AuthenticationPacket : IPacket
    {
        public byte Version { get; }
    }
}
