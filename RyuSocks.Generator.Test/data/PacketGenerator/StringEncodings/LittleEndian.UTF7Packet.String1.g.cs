using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace LittleEndian
{
    partial class UTF7Packet
    {
        private partial string String1
        {
            get
            {
                return Encoding.UTF7.GetString(this.AsSpan(2, 10));
            }
            set
            {
                Encoding.UTF7.GetBytes(value, this.AsSpan(2, 10));
            }
        }
    }
}
