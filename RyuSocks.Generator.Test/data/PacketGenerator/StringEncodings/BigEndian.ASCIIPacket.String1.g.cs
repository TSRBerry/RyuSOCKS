using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace BigEndian
{
    partial class ASCIIPacket
    {
        private partial string String1
        {
            get
            {
                return Encoding.ASCII.GetString(this.AsSpan(2, 10));
            }
            set
            {
                Encoding.ASCII.GetBytes(value, this.AsSpan(2, 10));
            }
        }
    }
}
