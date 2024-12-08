using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    partial ulong ULong1
    {
        get
        {
            return BitConverter.ToUInt64(this.AsSpan(6, 8));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(6, 8));
        }
    }
}
