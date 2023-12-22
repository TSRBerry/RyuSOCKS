using System;
using System.Net;

namespace RyuSocks.Packets
{
    public class UdpHeader : EndpointPacket
    {
        public ushort Reserved;
        public byte Fragment;

        public IPAddress DestinationAddress
        {
            get => Address;
            set => Address = value;
        }

        protected string DestinationDomainName
        {
            get => DomainName;
            set => DomainName = value;
        }

        protected ushort DestinationPort
        {
            get => Port;
            set => Port = value;
        }

        public override void FromArray(byte[] array)
        {
            Reserved = BitConverter.ToUInt16(array, 0);
            Fragment = array[2];

            base.FromArray(array);
        }

        public override byte[] ToArray()
        {
            // Reserved + Fragment
            const int BaseLength = 2 + 1;

            byte[] endpointArray = base.ToArray();
            byte[] array = new byte[BaseLength + endpointArray.Length];

            BitConverter.GetBytes(Reserved).CopyTo(array, 0);
            array[2] = Fragment;

            endpointArray.CopyTo(array, BaseLength);

            return array;
        }

        public override void Verify()
        {
            if (Reserved != 0)
            {
                throw new InvalidOperationException($"Reserved field is not 0: {Reserved}");
            }
        }
    }
}
