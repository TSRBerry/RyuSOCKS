using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace LittleEndian
{
    partial class UnicodePacket
    {
        private partial string String1
        {
            get
            {
                return Encoding.Unicode.GetString(this.AsSpan(2, 10));
            }
            set
            {
                Encoding.Unicode.GetBytes(value, this.AsSpan(2, 10));
            }
        }
    }
}
