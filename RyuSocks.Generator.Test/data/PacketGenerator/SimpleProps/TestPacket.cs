using RyuSocks.Packets;

public partial class TestPacket : Packet
{
    private int SecondOffset => 1;

    protected int AStringByteLength => 2;

    public int ReadWriteByteLength { get; set; } = 34;

    public byte Byte0 { get; set; }

    [PacketField(0)]
    public partial byte Byte1 { get; set; }

    [PacketField(1)]
    internal partial sbyte SByte1 { get; set; }

    [PacketField(2)]
    protected partial ushort UShort1 { get; set; }

    [PacketField(3)]
    private partial short Short1 { get; set; }

    [PacketField(4)]
    protected private partial uint UInt1 { get; set; }

    [PacketField(5)]
    partial int Int1 { get; set; }

    [PacketField(6)]
    partial ulong ULong1 { get; set; }

    [PacketField(7)]
    public partial long Long1 { get; set; }

    [PacketField(8, Length = 10)]
    private partial string String1 { get; set; }

    [PacketField(9, LengthMember = nameof(AStringByteLength))]
    partial string String2 { get; set; }

    [PacketField(10, LengthMember = nameof(ReadWriteByteLength))]
    protected partial string String3 { get; set; }
}
