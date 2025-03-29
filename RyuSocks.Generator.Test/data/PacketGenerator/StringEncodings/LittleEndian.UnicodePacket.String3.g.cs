using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleEndian
{
    partial class UnicodePacket
    {
        protected partial string String3
        {
            get
            {
                return Encoding.Unicode.GetString(this.AsSpan(14, this.ReadWriteByteLength));
            }
            set
            {
                this.ReadWriteByteLength = Encoding.Unicode.GetByteCount(value);
                Encoding.Unicode.GetBytes(value, this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
