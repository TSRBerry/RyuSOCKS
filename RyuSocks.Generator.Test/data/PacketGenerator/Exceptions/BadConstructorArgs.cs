using RyuSocks.Packets;

public partial class TestPacket : Packet
{
    private int SecondOffset => 1;

    [PacketField(1.5f)]
    public partial byte Byte1 { get; set; }
}
