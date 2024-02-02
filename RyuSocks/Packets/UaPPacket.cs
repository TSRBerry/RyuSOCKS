namespace RyuSocks.Packets
{
    public struct UaPPacket(string username, string password) : AuthenticationPacket
    {
        public byte Version => 0x01;
        public int UsernameLength = username.Length;
        public string Username = username;
        public int PasswordLength = password.Length;
        public string Password = password;
        public void FromArray(byte[] array){}
        public byte[] ToArray() { return null;}
        public void Verify(){}
    }
}
