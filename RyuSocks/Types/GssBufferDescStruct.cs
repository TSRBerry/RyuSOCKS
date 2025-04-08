namespace RyuSocks.Types
{
    public struct GssBufferDescStruct (ulong length = 0, byte[] value = null)
    {
        private ulong length = length;
        private byte[] value = value;
    }
}
