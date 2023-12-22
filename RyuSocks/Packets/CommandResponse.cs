using System.Net;

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

            base.FromArray(array);
        }

        public override byte[] ToArray()
        {
            // Version + ReplyField + Reserved
            const int BaseLength = 1 + 1 + 1;
            byte[] endpointArray = base.ToArray();
            byte[] array = new byte[BaseLength + endpointArray.Length];

            array[0] = Version;
            array[1] = (byte)ReplyField;
            array[2] = Reserved;

            endpointArray.CopyTo(array, BaseLength);

            return array;
        }
    }
}
