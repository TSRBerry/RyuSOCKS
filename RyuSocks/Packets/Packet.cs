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

        /// <summary>
        /// Validate the structure of the packet.
        /// This method is not supposed to verify the contents of the packet in depth.
        /// </summary>
        public abstract void Validate();
    }
}
