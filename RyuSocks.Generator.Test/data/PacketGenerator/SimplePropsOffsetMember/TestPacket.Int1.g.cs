using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    partial int Int1
    {
        get
        {
            return BitConverter.ToInt32(this.AsSpan(this.AnOffset, 4));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(this.AnOffset, 4));
        }
    }
}
