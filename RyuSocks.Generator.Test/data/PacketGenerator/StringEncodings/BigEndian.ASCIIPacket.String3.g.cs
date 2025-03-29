using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace BigEndian
{
    partial class ASCIIPacket
    {
        protected partial string String3
        {
            get
            {
                return Encoding.ASCII.GetString(this.AsSpan(14, this.ReadWriteByteLength));
            }
            set
            {
                this.ReadWriteByteLength = Encoding.ASCII.GetByteCount(value);
                Encoding.ASCII.GetBytes(value, this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
