using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    partial int[] IntArray1
    {
        get
        {
            int[] result = new int[64];
            
            for (int i = 0; i < 64; i++)
            {
                result[i] = BinaryPrimitives.ReadInt32LittleEndian(this.AsSpan(5 + (i * 4), 4));
            }
            
            return result;
        }
        set
        {
            for (int i = 0; i < 64; i++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(this.AsSpan(5 + (i * 4), 4), value[i]);
            }
        }
    }
}
