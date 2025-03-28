using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleEndian
{
    partial class ASCIIPacket
    {
        partial string String2
        {
            get
            {
                return Encoding.ASCII.GetString(this.AsSpan(12, this.AStringByteLength));
            }
            set
            {
                if (Encoding.ASCII.GetByteCount(value) != this.AStringByteLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Encoding.ASCII.GetByteCount(value), $"byte length of value must be equal to: {this.AStringByteLength}");
                }
                Encoding.ASCII.GetBytes(value, this.AsSpan(12, this.AStringByteLength));
            }
        }
    }
}
