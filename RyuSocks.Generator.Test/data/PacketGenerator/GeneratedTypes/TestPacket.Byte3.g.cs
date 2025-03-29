using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    protected partial GeneratedEnumByte[] Byte3
    {
        get
        {
            GeneratedEnumByte[] result = new GeneratedEnumByte[this.ALength];
            
            for (int i = 0; i < this.ALength; i++)
            {
                result[i] = (GeneratedEnumByte)this[this.AnOffset + i];
            }
            
            return result;
        }
        set
        {
            if (value.Length != this.ALength)
            {
                throw new ArgumentOutOfRangeException(nameof(value.Length), value.Length, $"{nameof(value.Length)} must be equal to: {this.ALength}");
            }
            
            for (int i = 0; i < this.ALength; i++)
            {
                this[this.AnOffset + i] = (byte)value[i];
            }
        }
    }
}
