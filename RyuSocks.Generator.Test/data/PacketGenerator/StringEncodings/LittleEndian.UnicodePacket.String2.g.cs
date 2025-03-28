using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleEndian
{
    partial class UnicodePacket
    {
        partial string String2
        {
            get
            {
                return Encoding.Unicode.GetString(this.AsSpan(12, this.AStringByteLength));
            }
            set
            {
                if (Encoding.Unicode.GetByteCount(value) != this.AStringByteLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Encoding.Unicode.GetByteCount(value), $"byte length of value must be equal to: {this.AStringByteLength}");
                }
                Encoding.Unicode.GetBytes(value, this.AsSpan(12, this.AStringByteLength));
            }
        }
    }
}
