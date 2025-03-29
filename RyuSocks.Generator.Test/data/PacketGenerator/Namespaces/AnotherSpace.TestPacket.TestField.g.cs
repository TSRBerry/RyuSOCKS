using RyuSocks.Packets;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace AnotherSpace
{
    partial class TestPacket
    {
        public partial byte TestField
        {
            get
            {
                return this[4];
            }
            set
            {
                this[4] = value;
            }
        }
    }
}
