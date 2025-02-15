using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    partial GeneratedEnumInt[] Int2
    {
        get
        {
            GeneratedEnumInt[] result = new GeneratedEnumInt[this.ALength];
            
            for (int i = 0; i < this.ALength; i++)
            {
                result[i] = (GeneratedEnumInt)BinaryPrimitives.ReadInt32LittleEndian(this.AsSpan(9 + (i * 4), 4));
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
                BinaryPrimitives.WriteInt32LittleEndian(this.AsSpan(9 + (i * 4), 4), (int)value[i]);
            }
        }
    }
}
