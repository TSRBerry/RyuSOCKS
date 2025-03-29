using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    protected private partial uint UInt1
    {
        get
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(this.AsSpan(this.AnOffset, 4));
        }
        set
        {
            BinaryPrimitives.WriteUInt32LittleEndian(this.AsSpan(this.AnOffset, 4), value);
        }
    }
}
