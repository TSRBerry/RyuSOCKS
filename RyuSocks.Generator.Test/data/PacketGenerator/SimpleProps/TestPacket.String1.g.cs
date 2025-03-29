using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    private partial string String1
    {
        get
        {
            return Encoding.ASCII.GetString(this.AsSpan(8, 10));
        }
        set
        {
            Encoding.ASCII.GetBytes(value, this.AsSpan(8, 10));
        }
    }
}
