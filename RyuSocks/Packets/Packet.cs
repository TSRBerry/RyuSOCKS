namespace RyuSocks.Packets
{
    public abstract class Packet
    {
        public byte[] Bytes { get; }

        public Packet()
        {
            Bytes = [];
        }

        public Packet(byte[] packetBytes)
        {
            Bytes = packetBytes;
        }

        public abstract void Validate();
    }
}
