using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    partial GeneratedEnumInt Int1
    {
        get
        {
            return (GeneratedEnumInt)BinaryPrimitives.ReadInt32LittleEndian(this.AsSpan(5, 4));
        }
        set
        {
            BinaryPrimitives.WriteInt32LittleEndian(this.AsSpan(5, 4), (int)value);
        }
    }
}
