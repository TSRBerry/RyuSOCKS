using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    public partial GeneratedEnumByte Byte1
    {
        get
        {
            return (GeneratedEnumByte)this[0];
        }
        set
        {
            this[0] = (byte)value;
        }
    }
}
