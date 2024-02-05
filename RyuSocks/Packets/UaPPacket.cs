namespace RyuSocks.Packets
{
    public struct UaPPacket(string username, string password) : IAuthenticationPacket
    {
        public byte Version { get; set; }
        public int UsernameLength = username.Length;
        public string Username = username;
        public int PasswordLength = password.Length;
        public string Password = password;
        public void FromArray(byte[] array)
        {
            Version = array[0];
            UsernameLength = array[1];
            Username = array[2..UsernameLength].ToString();
            PasswordLength = array[UsernameLength + 1];
            Password = array[(UsernameLength + 2) .. (UsernameLength + 2 + PasswordLength)].ToString();
        }
        public byte[] ToArray() { return null;}
        public void Verify(){}
    }
}
