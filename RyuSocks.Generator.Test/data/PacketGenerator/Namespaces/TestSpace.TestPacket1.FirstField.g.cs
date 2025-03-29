using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace TestSpace
{
    partial class TestPacket1
    {
        public partial byte FirstField
        {
            get
            {
                return this[1];
            }
            set
            {
                this[1] = value;
            }
        }
    }
}
