using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    private partial string[] StringArray1
    {
        get
        {
            string[] result = new string[44];
            
            for (int i = 0; i < 44; i++)
            {
                result[i] = Encoding.ASCII.GetString(this.AsSpan(8 + (i * 4), 4));
            }
            
            return result;
        }
        set
        {
            for (int i = 0; i < 44; i++)
            {
                Encoding.ASCII.GetBytes(value[i], this.AsSpan(8 + (i * 4), 4));
            }
        }
    }
}
