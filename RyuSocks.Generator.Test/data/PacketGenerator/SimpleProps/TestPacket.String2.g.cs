using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
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
            if (value.Length != this.AStringLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value.Length), value.Length, $"{nameof(value.Length)} must be equal to: {this.AStringLength}");
            }
            Encoding.ASCII.GetBytes(value, this.AsSpan(9, this.AStringLength));
        }
    }
}
