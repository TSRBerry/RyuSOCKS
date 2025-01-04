using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace LittleEndian
{
    partial class ASCIIPacket
    {
        public partial string String4
        {
            get
            {
                return Encoding.ASCII.GetString(this.AsSpan(48, 6));
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
                Encoding.ASCII.GetBytes(value, this.AsSpan(48, 6));
            }
        }
    }
}
