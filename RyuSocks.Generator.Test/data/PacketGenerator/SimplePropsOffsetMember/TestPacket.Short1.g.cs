using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    private partial short Short1
    {
        get
        {
            return BinaryPrimitives.ReadInt16LittleEndian(this.AsSpan(this.AnOffset, 2));
        }
        set
        {
            BinaryPrimitives.WriteInt16LittleEndian(this.AsSpan(this.AnOffset, 2), value);
        }
    }
}
