using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace BigEndian
{
    partial class UnicodePacket
    {
        private partial string String1
        {
            get
            {
                return Encoding.BigEndianUnicode.GetString(this.AsSpan(2, 10));
            }
            set
            {
                Encoding.BigEndianUnicode.GetBytes(value, this.AsSpan(2, 10));
            }
        }
    }
}
