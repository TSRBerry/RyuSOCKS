using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

partial class TestPacket
{
    internal partial GeneratedEnumSByte SByte1
    {
        get
        {
            return (GeneratedEnumSByte)(sbyte)this[1];
        }
        set
        {
            this[1] = (byte)value;
        }
    }
}
