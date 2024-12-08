using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    private partial short Short1
    {
        get
        {
            return BitConverter.ToInt16(this.AsSpan(3, 2));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(3, 2));
        }
    }
}
