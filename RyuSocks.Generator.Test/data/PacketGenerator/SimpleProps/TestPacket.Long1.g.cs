using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    public partial long Long1
    {
        get
        {
            return BinaryPrimitives.ReadInt64LittleEndian(this.AsSpan(7, 8));
        }
        set
        {
            BinaryPrimitives.WriteInt64LittleEndian(this.AsSpan(7, 8), value);
        }
    }
}
