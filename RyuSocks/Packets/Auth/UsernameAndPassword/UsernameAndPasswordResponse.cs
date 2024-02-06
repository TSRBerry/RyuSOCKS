namespace RyuSocks.Packets.Auth.UsernameAndPassword
{
    public class UsernameAndPasswordResponse : IPacket
    {
        public const byte Version = 0x01;
        public readonly byte Status;

        public UsernameAndPasswordResponse(byte status)
        {
            Status = status;
        }

        public void FromArray(byte[] array){}
        public byte[] ToArray()
        {
            byte[] array = new byte[2];
            array[0] = Version;
            array[1] = Status;
            return array;
        }
        public void Verify(){}
    }
}
