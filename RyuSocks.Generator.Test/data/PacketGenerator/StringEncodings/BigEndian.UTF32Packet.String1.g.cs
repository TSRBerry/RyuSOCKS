using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace BigEndian
{
    partial class UTF32Packet
    {
        private partial string String1
        {
            get
            {
                byte[] stringArray = this.AsSpan(2, 10).ToArray();
                Array.Reverse(stringArray);
                return Encoding.UTF32.GetString(stringArray);
            }
            set
            {
                byte[] byteArray = Encoding.UTF32.GetBytes(value);
                Array.Reverse(byteArray);
                byteArray.CopyTo(this.AsSpan(2, 10));
            }
        }
    }
}
