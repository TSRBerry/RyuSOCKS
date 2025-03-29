using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    public partial GeneratedEnumByte Byte2
    {
        get
        {
            return (GeneratedEnumByte)this[this.AnOffset];
        }
        set
        {
            this[this.AnOffset] = (byte)value;
        }
    }
}
