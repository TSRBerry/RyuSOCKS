using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    protected private partial uint UInt1
    {
        get
        {
            return BitConverter.ToUInt32(this.AsSpan(4, 4));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(4, 4));
        }
    }
}
