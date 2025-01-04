using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace BigEndian
{
    partial class UnicodePacket
    {
        protected partial string String3
        {
            get
            {
                return Encoding.BigEndianUnicode.GetString(this.AsSpan(14, this.ReadWriteByteLength));
            }
            set
            {
                this.ReadWriteByteLength = Encoding.Unicode.GetByteCount(value);
                Encoding.BigEndianUnicode.GetBytes(value, this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
