using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace LittleEndian
{
    partial class UTF7Packet
    {
        partial string String2
        {
            get
            {
                return Encoding.UTF7.GetString(this.AsSpan(12, this.AStringByteLength));
            }
            set
            {
                if (Encoding.UTF7.GetByteCount(value) != this.AStringByteLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Encoding.UTF7.GetByteCount(value), $"byte length of value must be equal to: {this.AStringByteLength}");
                }
                Encoding.UTF7.GetBytes(value, this.AsSpan(12, this.AStringByteLength));
            }
        }
    }
}
