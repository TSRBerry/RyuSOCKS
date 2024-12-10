using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Text;

namespace AnotherSpace
{
    partial class TestPacket2
    {
        private partial byte SecondField
        {
            get
            {
                return this[2];
            }
            set
            {
                this[2] = value;
            }
        }
    }
}
