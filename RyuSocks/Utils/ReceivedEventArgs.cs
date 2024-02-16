using System;

namespace RyuSocks.Utils
{
    public class ReceivedEventArgs : EventArgs
    {
        public byte[] Buffer { get; init; }
        public long Offset { get; init; }
        public long Size { get; init; }
    }
}
