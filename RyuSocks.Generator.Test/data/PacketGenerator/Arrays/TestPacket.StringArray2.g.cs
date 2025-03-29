using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    partial string[] StringArray2
    {
        get
        {
            string[] result = new string[this.AStringArrayLength];
            
            for (int i = 0; i < this.AStringArrayLength; i++)
            {
                result[i] = Encoding.ASCII.GetString(this.AsSpan(9 + (i * 2), 2));
            }
            
            return result;
        }
        set
        {
            if (value.Length != this.AStringArrayLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value.Length), value.Length, $"{nameof(value.Length)} must be equal to: {this.AStringArrayLength}");
            }
            
            for (int i = 0; i < this.AStringArrayLength; i++)
            {
                Encoding.ASCII.GetBytes(value[i], this.AsSpan(9 + (i * 2), 2));
            }
        }
    }
}
