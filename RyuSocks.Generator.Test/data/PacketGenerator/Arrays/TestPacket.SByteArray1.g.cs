using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    internal partial sbyte[] SByteArray1
    {
        get
        {
            sbyte[] result = new sbyte[11];
            
            for (int i = 0; i < 11; i++)
            {
                result[i] = (sbyte)this[1 + i];
            }
            
            return result;
        }
        set
        {
            for (int i = 0; i < 11; i++)
            {
                this[1 + i] = (byte)value[i];
            }
        }
    }
}
