namespace RyuSocks
{
    public enum ProxyCommand : byte
    {
        Connect = 0x01,
        Bind,
        UdpAssociate,
    }
}
