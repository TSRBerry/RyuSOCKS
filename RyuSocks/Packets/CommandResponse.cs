using System;
using System.Net;
using System.Text;

namespace RyuSocks.Packets
{
    public class CommandResponse : CommandPacket
    {
        public ReplyField ReplyField;

        public IPAddress BoundAddress
        {
            get => Address;
            set => Address = value;
        }

        public string BoundDomainName
        {
            get => DomainName;
            set => DomainName = value;
        }

        public ushort BoundPort
        {
            get => Port;
            set => Port = value;
        }

        public override void FromArray(byte[] array)
        {
            Version = array[0];
            ReplyField = (ReplyField)array[1];
            Reserved = array[2];
            AddressType = (AddressType)array[3];

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                    BoundAddress = new IPAddress(array[4..8]);
                    break;
                case AddressType.DomainName:
                    BoundDomainName = Encoding.ASCII.GetString(array, 5, array[4]);
                    break;
                case AddressType.Ipv6Address:
                    BoundAddress = new IPAddress(array[4..20]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            BoundPort = BitConverter.ToUInt16(array, array.Length - 2);
        }

        public override byte[] ToArray()
        {
            // Version + ReplyField + Reserved + AddressType + BoundPort
            const int BaseLength = 1 + 1 + 1 + 1 + 2;
            byte[] array;

            switch (AddressType)
            {
                case AddressType.Ipv4Address:
                {
                    array = new byte[BaseLength + 4];

                    AssignElements();
                    BoundAddress.GetAddressBytes().CopyTo(array, 4);
                    break;
                }
                case AddressType.DomainName:
                {
                    array = new byte[BaseLength + BoundDomainName.Length + 1];

                    AssignElements();
                    array[4] = (byte)BoundDomainName.Length;
                    Encoding.ASCII.GetBytes(BoundDomainName).CopyTo(array, 5);
                    break;
                }
                case AddressType.Ipv6Address:
                {
                    array = new byte[BaseLength + 16];

                    AssignElements();
                    BoundAddress.GetAddressBytes().CopyTo(array, 4);
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }

            return array;

            void AssignElements()
            {
                array[0] = Version;
                array[1] = (byte)ReplyField;
                array[2] = Reserved;
                array[3] = (byte)AddressType;
                BitConverter.GetBytes(_port).CopyTo(array, array.Length - 2);
            }
        }
    }
}
