using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

partial class TestPacket
{
    internal partial sbyte SByte1
    {
        get
        {
            return (sbyte)this[1];
        }
        set
        {
            this[1] = (byte)value;
        }
    }
}
