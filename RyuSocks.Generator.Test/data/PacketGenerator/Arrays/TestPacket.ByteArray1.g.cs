using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    public partial byte[] ByteArray1
    {
        get
        {
            byte[] result = new byte[60];
            
            for (int i = 0; i < 60; i++)
            {
                result[i] = this[0 + i];
            }
            
            return result;
        }
        set
        {
            for (int i = 0; i < 60; i++)
            {
                this[0 + i] = value[i];
            }
        }
    }
}
