using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace BigEndian
{
    partial class UTF32Packet
    {
        partial string String2
        {
            get
            {
                byte[] stringArray = this.AsSpan(12, this.AStringByteLength).ToArray();
                Array.Reverse(stringArray);
                return Encoding.UTF32.GetString(stringArray);
            }
            set
            {
                if (Encoding.UTF32.GetByteCount(value) != this.AStringByteLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Encoding.UTF32.GetByteCount(value), $"byte length of value must be equal to: {this.AStringByteLength}");
                }
                byte[] byteArray = Encoding.UTF32.GetBytes(value);
                Array.Reverse(byteArray);
                byteArray.CopyTo(this.AsSpan(12, this.AStringByteLength));
            }
        }
    }
}
