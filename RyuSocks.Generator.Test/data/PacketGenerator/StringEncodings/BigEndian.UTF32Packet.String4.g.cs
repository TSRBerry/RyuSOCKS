using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace BigEndian
{
    partial class UTF32Packet
    {
        public partial string String4
        {
            get
            {
                byte[] stringArray = this.AsSpan(48, 6).ToArray();
                Array.Reverse(stringArray);
                return Encoding.UTF32.GetString(stringArray);
            }
            set
            {
                if (value.Length < 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(value.Length), value.Length, $"{nameof(value.Length)} must be larger or equal to: {2}");
                }
                if (value.Length > 6)
                {
                    throw new ArgumentOutOfRangeException(nameof(value.Length), value.Length, $"{nameof(value.Length)} must be smaller or equal to: {6}");
                }
                byte[] byteArray = Encoding.UTF32.GetBytes(value);
                Array.Reverse(byteArray);
                byteArray.CopyTo(this.AsSpan(48, 6));
            }
        }
    }
}
