using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    public partial byte Byte1
    {
        get
        {
            return this[this.AnOffset];
        }
        set
        {
            this[this.AnOffset] = value;
        }
    }
}
