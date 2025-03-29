using RyuSocks.Packets;

public partial class TestPacket : Packet
{
    private int AnOffset => 2;
    protected int ALength => 3;

    [PacketField(0, AssumeGeneratedEnumType="byte")]
    public partial GeneratedEnumByte Byte1 { get; set; }

    [PacketField(1, AssumeGeneratedEnumType="SBYTE")]
    internal partial GeneratedEnumSByte SByte1 { get; set; }

    [PacketField(2, AssumeGeneratedEnumType="UsHoRt")]
    protected partial GeneratedEnumUShort UShort1 { get; set; }

    [PacketField(3, AssumeGeneratedEnumType="sHoRt")]
    private partial GeneratedEnumShort Short1 { get; set; }

    [PacketField(4, AssumeGeneratedEnumType="uint")]
    protected private partial GeneratedEnumUInt UInt1 { get; set; }

    [PacketField(5, AssumeGeneratedEnumType="int")]
    partial GeneratedEnumInt Int1 { get; set; }

    [PacketField(6, AssumeGeneratedEnumType="ulong")]
    partial GeneratedEnumULong ULong1 { get; set; }

    [PacketField(7, AssumeGeneratedEnumType="long")]
    public partial GeneratedEnumLong Long1 { get; set; }

    [PacketField(nameof(AnOffset), AssumeGeneratedEnumType="byte")]
    public partial GeneratedEnumByte Byte2 { get; set; }

    [PacketField(9, LengthMember = nameof(ALength), AssumeGeneratedEnumType="int")]
    partial GeneratedEnumInt[] Int2 { get; set; }

    [PacketField(nameof(AnOffset), LengthMember = nameof(ALength), AssumeGeneratedEnumType="byte")]
    protected partial GeneratedEnumByte[] Byte3 { get; set; }
}
