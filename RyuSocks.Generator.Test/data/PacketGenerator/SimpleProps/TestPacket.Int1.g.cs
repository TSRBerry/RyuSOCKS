using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    partial int Int1
    {
        get
        {
            return BinaryPrimitives.ReadInt32LittleEndian(this.AsSpan(5, 4));
        }
        set
        {
            BinaryPrimitives.WriteInt32LittleEndian(this.AsSpan(5, 4), value);
        }
    }
}
