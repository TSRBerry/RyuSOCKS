using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleEndian
{
    partial class UTF32Packet
    {
        private partial string String1
        {
            get
            {
                return Encoding.UTF32.GetString(this.AsSpan(2, 10));
            }
            set
            {
                Encoding.UTF32.GetBytes(value, this.AsSpan(2, 10));
            }
        }
    }
}
