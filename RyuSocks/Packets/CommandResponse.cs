using System.Net;

namespace RyuSocks.Packets
{
    public class CommandResponse : CommandPacket
    {
        // Version

        public ReplyField ReplyField
        {
            get
            {
                return (ReplyField)Bytes[1];
            }
            set
            {
                Bytes[1] = (byte)value;
            }
        }

        // Reserved

        // AddressType

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
    }
}
