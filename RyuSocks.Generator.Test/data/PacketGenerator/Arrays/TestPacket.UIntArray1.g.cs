using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    protected private partial uint[] UIntArray1
    {
        get
        {
            uint[] result = new uint[32];
            
            for (int i = 0; i < 32; i++)
            {
                result[i] = BinaryPrimitives.ReadUInt32LittleEndian(this.AsSpan(4 + (i * 4), 4));
            }
            
            return result;
        }
        set
        {
            for (int i = 0; i < 32; i++)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(this.AsSpan(4 + (i * 4), 4), value[i]);
            }
        }
    }
}
