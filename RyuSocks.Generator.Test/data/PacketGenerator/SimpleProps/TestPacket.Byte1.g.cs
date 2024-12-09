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
            return this[0];
        }
        set
        {
            this[0] = value;
        }
    }
}
