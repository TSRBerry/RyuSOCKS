using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    partial ulong ULong1
    {
        get
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(this.AsSpan(this.AnOffset, 8));
        }
        set
        {
            BinaryPrimitives.WriteUInt64LittleEndian(this.AsSpan(this.AnOffset, 8), value);
        }
    }
}
