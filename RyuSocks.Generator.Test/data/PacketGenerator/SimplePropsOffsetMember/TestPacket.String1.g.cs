using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    private partial string String1
    {
        get
        {
            return Encoding.ASCII.GetString(this.AsSpan(this.AnOffset, 10));
        }
        set
        {
            Encoding.ASCII.GetBytes(value, this.AsSpan(this.AnOffset, 10));
        }
    }
}
