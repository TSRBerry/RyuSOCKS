using RyuSocks.Packets;

internal partial class TestPacket : Packet
{
    private int AnOffset => 2;

    protected int AStringByteLength => 2;

    public int ReadWriteByteLength { get; set; } = 34;

    [PacketField(nameof(AnOffset))]
    public partial byte Byte1 { get; set; }

    [PacketField(nameof(AnOffset))]
    internal partial sbyte SByte1 { get; set; }

    [PacketField(nameof(AnOffset))]
    protected partial ushort UShort1 { get; set; }

    [PacketField(nameof(AnOffset))]
    private partial short Short1 { get; set; }

    [PacketField(nameof(AnOffset))]
    protected private partial uint UInt1 { get; set; }

    [PacketField(nameof(AnOffset))]
    partial int Int1 { get; set; }

    [PacketField(nameof(AnOffset))]
    partial ulong ULong1 { get; set; }

    [PacketField(nameof(AnOffset))]
    public partial long Long1 { get; set; }

    [PacketField(nameof(AnOffset), Length = 10)]
    private partial string String1 { get; set; }

    [PacketField(nameof(AnOffset), LengthMember = nameof(AStringByteLength))]
    partial string String2 { get; set; }

    [PacketField(nameof(AnOffset), LengthMember = nameof(ReadWriteByteLength))]
    protected partial string String3 { get; set; }
}
