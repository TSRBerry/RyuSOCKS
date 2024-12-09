using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    protected partial ushort UShort1
    {
        get
        {
            return BitConverter.ToUInt16(this.AsSpan(this.AnOffset, 2));
        }
        set
        {
            BitConverter.GetBytes(value).CopyTo(this.AsSpan(this.AnOffset, 2));
        }
    }
}
