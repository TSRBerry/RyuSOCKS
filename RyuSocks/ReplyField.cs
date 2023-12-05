namespace RyuSocks
{
    public enum ReplyField : byte
    {
        Succeeded,
        ServerFailure,
        ConnectionNotAllowed,
        NetworkUnreachable,
        HostUnreachable,
        ConnectionRefused,
        TTLExpired,
        CommandNotSupported,
        AddressTypeNotSupported,
    }
}
