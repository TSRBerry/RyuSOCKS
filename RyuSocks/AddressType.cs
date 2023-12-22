namespace RyuSocks
{
    public enum AddressType : byte
    {
        Ipv4Address = 0x01,
        DomainName = 0x03,
        Ipv6Address,
    }
}
