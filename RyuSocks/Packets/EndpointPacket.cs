using System;
using System.Net;
using System.Text;

namespace RyuSocks.Packets
{
    public abstract class EndpointPacket : IPacket
    {
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

        public virtual void FromArray(byte[] array)
        {
            AddressType = (AddressType)array[3];

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                    Address = new IPAddress(array[4..8]);
                    Port = BitConverter.ToUInt16(array, 9);
                    break;
                case AddressType.DomainName:
                    DomainName = Encoding.ASCII.GetString(array, 5, array[4]);
                    Port = BitConverter.ToUInt16(array, 5 + array[4]);
                    break;
                case AddressType.Ipv6Address:
                    Address = new IPAddress(array[4..20]);
                    Port = BitConverter.ToUInt16(array, 21);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual byte[] ToArray()
        {
            byte[] array;

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                {
                    // AddressType + Address + Port
                    array = new byte[1 + 4 + 2];

                    array[0] = (byte)AddressType;
                    Address.GetAddressBytes().CopyTo(array, 1);
                    BitConverter.GetBytes(_port).CopyTo(array, 5);

                    break;
                }
                case AddressType.DomainName:
                {
                    // AddressType + DomainNameLength + DomainName + Port
                    array = new byte[1 + 1 + DomainName.Length + 2];

                    array[0] = (byte)AddressType;
                    array[1] = (byte)DomainName.Length;
                    Encoding.ASCII.GetBytes(DomainName).CopyTo(array, 2);
                    BitConverter.GetBytes(_port).CopyTo(array, 1 + 1 + DomainName.Length);

                    break;
                }
                case AddressType.Ipv6Address:
                {
                    // AddressType + Address + Port
                    array = new byte[1 + 16 + 2];

                    array[0] = (byte)AddressType;
                    Address.GetAddressBytes().CopyTo(array, 1);
                    BitConverter.GetBytes(_port).CopyTo(array, 17);

                    break;
                }
                default:
                    throw new InvalidOperationException();
            }

            return array;
        }

        public abstract void Verify();
    }
}
