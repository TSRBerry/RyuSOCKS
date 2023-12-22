using System.Net;

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

            base.FromArray(array);
        }

        public override byte[] ToArray()
        {
            // Version + Command + Reserved
            const int BaseLength = 1 + 1 + 1;
            byte[] endpointArray = base.ToArray();
            byte[] array = new byte[BaseLength + endpointArray.Length];

            array[0] = Version;
            array[1] = (byte)Command;
            array[2] = Reserved;

            endpointArray.CopyTo(array, BaseLength);

            return array;
        }
    }
}
