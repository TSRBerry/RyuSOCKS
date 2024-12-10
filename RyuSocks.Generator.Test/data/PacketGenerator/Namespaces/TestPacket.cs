using RyuSocks.Packets;

namespace TestSpace
{
    public partial class TestPacket : Packet
    {
        [PacketField(0)]
        internal partial byte TestField { get; set; }
    }

    partial class TestPacket1 : Packet
    {
        [PacketField(1)]
        public partial byte FirstField { get; set; }
    }
}

namespace AnotherSpace
{
    partial class TestPacket : Packet
    {
        [PacketField(4)]
        public partial byte TestField { get; set; }
    }

    internal partial class TestPacket2 : Packet
    {
        [PacketField(2)]
        private partial byte SecondField { get; set; }
    }
}
