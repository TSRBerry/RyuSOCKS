namespace RyuSocks.Types
{
    public struct GssChannelBindingsStruct
    {
        private GssApiAddresstype initiatorAddrtype;
        private GssBufferDescStruct initiatorAddress;
        private GssApiAddresstype acceptorAddrtype;
        private GssBufferDescStruct acceptorAddress;
        private GssBufferDescStruct applicationData;
    }
}
