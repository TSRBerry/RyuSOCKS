using System;
using System.Net;
using System.Text;

namespace RyuSocks.Packets
{
    public class CommandRequest : CommandPacket
    {
        public ProxyCommand Command;

        public IPAddress DestinationAddress
        {
            get => Address;
            set => Address = value;
        }

        public string DestinationDomainName
        {
            get => DomainName;
            set => DomainName = value;
        }

        public ushort DestinationPort
        {
            get => Port;
            set => Port = value;
        }

        public override void FromArray(byte[] array)
        {
            Version = array[0];
            Command = (ProxyCommand)array[1];
            Reserved = array[2];
            AddressType = (AddressType)array[3];

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                    DestinationAddress = new IPAddress(array[4..8]);
                    break;
                case AddressType.DomainName:
                    DestinationDomainName = Encoding.ASCII.GetString(array, 5, array[4]);
                    break;
                case AddressType.Ipv6Address:
                    DestinationAddress = new IPAddress(array[4..20]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DestinationPort = BitConverter.ToUInt16(array, array.Length - 2);
        }

        public override byte[] ToArray()
        {
            // Version + Command + Reserved + AddressType + DestinationPort
            const int BaseLength = 1 + 1 + 1 + 1 + 2;
            byte[] array;

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                {
                    array = new byte[BaseLength + 4];

                    AssignElements();
                    DestinationAddress.GetAddressBytes().CopyTo(array, 4);
                    break;
                }
                case AddressType.DomainName:
                {
                    array = new byte[BaseLength + DestinationDomainName.Length + 1];

                    AssignElements();
                    array[4] = (byte)DestinationDomainName.Length;
                    Encoding.ASCII.GetBytes(DestinationDomainName).CopyTo(array, 5);
                    break;
                }
                case AddressType.Ipv6Address:
                {
                    array = new byte[BaseLength + 16];

                    AssignElements();
                    DestinationAddress.GetAddressBytes().CopyTo(array, 4);
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }

            return array;

            void AssignElements()
            {
                array[0] = Version;
                array[1] = (byte)Command;
                array[2] = Reserved;
                array[3] = (byte)AddressType;
                BitConverter.GetBytes(_port).CopyTo(array, array.Length - 2);
            }
        }
    }
}
