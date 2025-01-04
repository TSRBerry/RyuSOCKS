using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    protected partial string String3
    {
        get
        {
            return Encoding.ASCII.GetString(this.AsSpan(10, this.ReadWriteByteLength));
        }
        set
        {
            this.ReadWriteByteLength = Encoding.ASCII.GetByteCount(value);
            Encoding.ASCII.GetBytes(value, this.AsSpan(10, this.ReadWriteByteLength));
        }
    }
}
