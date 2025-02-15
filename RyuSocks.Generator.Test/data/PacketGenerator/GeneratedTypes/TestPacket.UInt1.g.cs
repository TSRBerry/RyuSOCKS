using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    protected private partial GeneratedEnumUInt UInt1
    {
        get
        {
            return (GeneratedEnumUInt)BinaryPrimitives.ReadUInt32LittleEndian(this.AsSpan(4, 4));
        }
        set
        {
            BinaryPrimitives.WriteUInt32LittleEndian(this.AsSpan(4, 4), (uint)value);
        }
    }
}
