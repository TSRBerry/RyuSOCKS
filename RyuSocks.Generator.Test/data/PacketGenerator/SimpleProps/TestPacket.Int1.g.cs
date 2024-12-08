using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    partial int Int1
    {
        get
        {
            return BitConverter.ToInt32(this.AsSpan(5, 4));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(5, 4));
        }
    }
}
