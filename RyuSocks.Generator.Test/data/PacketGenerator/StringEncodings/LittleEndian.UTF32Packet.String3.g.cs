using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleEndian
{
    partial class UTF32Packet
    {
        protected partial string String3
        {
            get
            {
                return Encoding.UTF32.GetString(this.AsSpan(14, this.ReadWriteByteLength));
            }
            set
            {
                this.ReadWriteByteLength = Encoding.UTF32.GetByteCount(value);
                Encoding.UTF32.GetBytes(value, this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
