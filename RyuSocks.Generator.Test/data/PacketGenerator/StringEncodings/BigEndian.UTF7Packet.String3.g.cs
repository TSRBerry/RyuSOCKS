using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace BigEndian
{
    partial class UTF7Packet
    {
        protected partial string String3
        {
            get
            {
                return Encoding.UTF7.GetString(this.AsSpan(14, this.ReadWriteByteLength));
            }
            set
            {
                this.ReadWriteByteLength = Encoding.UTF7.GetByteCount(value);
                Encoding.UTF7.GetBytes(value, this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
