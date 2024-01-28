using System;

namespace RyuSocks.Utils
{
    public class ReceivedEventArgs : EventArgs
    {
        public byte[] Buffer { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }
    }
}
