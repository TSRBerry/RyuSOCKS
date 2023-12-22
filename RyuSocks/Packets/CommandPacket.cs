using System;
using System.Net;

namespace RyuSocks.Packets
{
    public abstract class CommandPacket
    {
        public byte Version;
        // ProxyCommand or ReplyField
        public byte Reserved;
        public AddressType AddressType;
        
        protected uint _ipv4Address;
        protected string _domainName;
        protected Array16<byte> _ipv6Address;
        
        protected ushort _port;
        
        protected IPAddress Address
        {
            get
            {
                return AddressType switch
                {
                    AddressType.Ipv4Address => new IPAddress(BitConverter.GetBytes(_ipv4Address)),
                    AddressType.DomainName => null,
                    AddressType.Ipv6Address => new IPAddress(_ipv6Address.AsSpan()),
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
            set
            {
                switch (AddressType)
                {
                    case AddressType.Ipv4Address:
                        _ipv4Address = BitConverter.ToUInt32(value.GetAddressBytes());
                        return;
                    case AddressType.DomainName:
                        throw new InvalidOperationException();
                    case AddressType.Ipv6Address:
                        _ipv6Address = new Array16<byte>();
                        value.GetAddressBytes().CopyTo(_ipv6Address.AsSpan());
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected string DomainName
        {
            get
            {
                return AddressType switch
                {
                    AddressType.Ipv4Address => throw new InvalidOperationException(),
                    AddressType.DomainName => _domainName,
                    AddressType.Ipv6Address => throw new InvalidOperationException(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
            set
            {
                switch (AddressType)
                {
                    case AddressType.Ipv4Address:
                        throw new InvalidOperationException();
                    case AddressType.DomainName:
                        if (value.Length > 255)
                        {
                            throw new ArgumentException("FQDN is too long.");
                        }
                        _domainName = value;
                        return;
                    case AddressType.Ipv6Address:
                        throw new InvalidOperationException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected ushort Port
        {
            get
            {
                byte[] portBytes = BitConverter.GetBytes(_port);
                Array.Reverse(portBytes);
                return BitConverter.ToUInt16(portBytes);
            }
            set
            {
                byte[] portBytes = BitConverter.GetBytes(value);
                Array.Reverse(portBytes);
                _port = BitConverter.ToUInt16(portBytes);
            }
        }
        
        public abstract void FromArray(byte[] array);
        public abstract byte[] ToArray();
        
        public virtual void Verify()
        {
            if (Version != ProxyConsts.Version)
            {
                throw new InvalidOperationException(
                    $"Version field is invalid: {Version:X} (Expected: {ProxyConsts.Version:X})");
            }

            if (Reserved != 0)
            {
                throw new InvalidOperationException($"Reserved field is not 0: {Reserved}");
            }
        }
    }
}
