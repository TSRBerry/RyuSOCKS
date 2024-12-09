using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    protected partial ushort UShort1
    {
        get
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(this.AsSpan(this.AnOffset, 2));
        }
        set
        {
            BinaryPrimitives.WriteUInt16LittleEndian(this.AsSpan(this.AnOffset, 2), value);
        }
    }
}
