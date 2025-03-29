using RyuSocks.Packets;

public partial class TestPacket : Packet
{
    protected int AStringArrayLength => 20;

    public int ReadWriteByteLength { get; set; } = 34;

    [PacketField(0, Length = 60)]
    public partial byte[] ByteArray1 { get; set; }

    [PacketField(1, Length = 11)]
    internal partial sbyte[] SByteArray1 { get; set; }

    [PacketField(4, Length = 32)]
    protected private partial uint[] UIntArray1 { get; set; }

    [PacketField(5, Length = 64)]
    partial int[] IntArray1 { get; set; }

    [PacketField(8, Length = 44, ElementSize = 4)]
    private partial string[] StringArray1 { get; set; }

    [PacketField(9, LengthMember = nameof(AStringArrayLength), ElementSize = 2)]
    partial string[] StringArray2 { get; set; }
}
