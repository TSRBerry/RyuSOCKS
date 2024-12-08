using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    public partial long Long1
    {
        get
        {
            return BitConverter.ToInt64(this.AsSpan(7, 8));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(7, 8));
        }
    }
}
