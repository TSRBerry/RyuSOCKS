using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace BigEndian
{
    partial class UTF32Packet
    {
        protected partial string String3
        {
            get
            {
                byte[] stringArray = this.AsSpan(14, this.ReadWriteByteLength).ToArray();
                Array.Reverse(stringArray);
                return Encoding.UTF32.GetString(stringArray);
            }
            set
            {
                this.ReadWriteByteLength = Encoding.UTF32.GetByteCount(value);
                byte[] byteArray = Encoding.UTF32.GetBytes(value);
                Array.Reverse(byteArray);
                byteArray.CopyTo(this.AsSpan(14, this.ReadWriteByteLength));
            }
        }
    }
}
