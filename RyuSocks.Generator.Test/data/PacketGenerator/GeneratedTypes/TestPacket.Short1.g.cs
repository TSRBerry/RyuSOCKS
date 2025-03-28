using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    private partial GeneratedEnumShort Short1
    {
        get
        {
            return (GeneratedEnumShort)BinaryPrimitives.ReadInt16LittleEndian(this.AsSpan(3, 2));
        }
        set
        {
            BinaryPrimitives.WriteInt16LittleEndian(this.AsSpan(3, 2), (short)value);
        }
    }
}
