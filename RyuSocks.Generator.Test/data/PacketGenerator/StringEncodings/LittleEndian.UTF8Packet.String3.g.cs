using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace LittleEndian
{
    partial class UTF8Packet
    {
        protected partial string String3
        {
            get
            {
                return Encoding.UTF8.GetString(this.AsSpan(14, this.ReadWriteByteLength));
            }
            set
            {
                this.ReadWriteByteLength = Encoding.UTF8.GetByteCount(value);
                Encoding.UTF8.GetBytes(value, this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
