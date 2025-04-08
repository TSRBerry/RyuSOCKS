using System.Collections.Generic;

namespace RyuSocks.Types
{
    public struct GssNameT
    {
        // Not really sure how to do this in C#, for reference: https://datatracker.ietf.org/doc/html/rfc2744.html#section-3.10
        // the code is supposed to have names referring to each valid OID
        Dictionary<GssBufferDescStruct, GssOIDDescStruct> namesWithOID;
    }
}
