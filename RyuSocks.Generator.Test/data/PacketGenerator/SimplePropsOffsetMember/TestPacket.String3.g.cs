using RyuSocks.Packets;
using System;
using System.Text;

partial class TestPacket
{
    protected partial string String3
    {
        get
        {
            return Encoding.ASCII.GetString(this.AsSpan(this.AnOffset, this.ReadWriteLength));
        }
        set
        {
            this.ReadWriteLength = value.Length;
            Encoding.ASCII.GetBytes(value, this.AsSpan(this.AnOffset, this.ReadWriteLength));
        }
    }
}
