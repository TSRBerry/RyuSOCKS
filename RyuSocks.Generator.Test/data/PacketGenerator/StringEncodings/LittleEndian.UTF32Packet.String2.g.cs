using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace LittleEndian
{
    partial class UTF32Packet
    {
        partial string String2
        {
            get
            {
                return Encoding.UTF32.GetString(this.AsSpan(12, this.AStringByteLength));
            }
            set
            {
                if (Encoding.UTF32.GetByteCount(value) != this.AStringByteLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Encoding.UTF32.GetByteCount(value), $"byte length of value must be equal to: {this.AStringByteLength}");
                }
                Encoding.UTF32.GetBytes(value, this.AsSpan(12, this.AStringByteLength));
            }
        }
    }
}
