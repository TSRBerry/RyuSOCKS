using System.Net;

namespace RyuSocks.Packets
{
    public abstract class CommandRequest : CommandPacket
    {
        // Version

        public ProxyCommand Command
        {
            get
            {
                return (ProxyCommand)Bytes[1];
            }
            set
            {
                Bytes[1] = (byte)value;
            }
        }

        // Reserved

        // AddressType

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
    }
}
