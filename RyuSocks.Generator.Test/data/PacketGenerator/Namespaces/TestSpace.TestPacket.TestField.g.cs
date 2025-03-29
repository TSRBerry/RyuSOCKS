using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace TestSpace
{
    partial class TestPacket
    {
        internal partial byte TestField
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }
    }
}
