using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    partial ulong ULong1
    {
        get
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(this.AsSpan(6, 8));
        }
        set
        {
            BinaryPrimitives.WriteUInt64LittleEndian(this.AsSpan(6, 8), value);
        }
    }
}
