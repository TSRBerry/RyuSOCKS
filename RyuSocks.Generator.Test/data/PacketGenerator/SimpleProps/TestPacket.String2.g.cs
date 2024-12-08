using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    partial string String2
    {
        get
        {
            return Encoding.ASCII.GetString(this.AsSpan(9, this.AStringLength));
        }
        set
        {
            Encoding.ASCII.GetBytes(value, this.AsSpan(9, this.AStringLength));
        }
    }
}
