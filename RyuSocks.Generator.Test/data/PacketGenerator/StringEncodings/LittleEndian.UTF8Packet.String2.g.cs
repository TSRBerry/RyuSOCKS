using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleEndian
{
    partial class UTF8Packet
    {
        partial string String2
        {
            get
            {
                return Encoding.UTF8.GetString(this.AsSpan(12, this.AStringByteLength));
            }
            set
            {
                if (Encoding.UTF8.GetByteCount(value) != this.AStringByteLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Encoding.UTF8.GetByteCount(value), $"byte length of value must be equal to: {this.AStringByteLength}");
                }
                Encoding.UTF8.GetBytes(value, this.AsSpan(12, this.AStringByteLength));
            }
        }
    }
}
