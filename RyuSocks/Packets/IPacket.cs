namespace RyuSocks.Packets
{
    public interface IPacket
    {
        public void FromArray(byte[] array);
        public byte[] ToArray();
        public void Verify();
    }
}
