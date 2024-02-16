using System;
using System.Net;

namespace RyuSocks.Packets
{
    public class UdpHeader : EndpointPacket
    {
        public ushort Reserved
        {
            get
            {
                return BitConverter.ToUInt16(Bytes.AsSpan(0, 2));
            }
            set
            {
                BitConverter.GetBytes(value).CopyTo(Bytes.AsSpan(0, 2));
            }
        }

        public byte Fragment
        {
            get
            {
                return Bytes[2];
            }
            set
            {
                Bytes[2] = value;
            }
        }

        // AddressType

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

        public override void Validate()
        {
            if (Reserved != 0)
            {
                throw new InvalidOperationException($"Reserved field is not 0: {Reserved}");
            }
        }
    }
}
