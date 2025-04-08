namespace RyuSocks.Types
{
    public enum GssApiStatus : uint
    {
        // MAJOR STATUS CODES
        GssComplete,
        GssContinueNeeded,
        GssCNoContext,
        GssFailure,
        
        // ADDITIONAL MAJOR STATUS CODES IN CONJUNCTION WITH THE ONES ABOVE
        GssDuplicateToken,
        GssOldToken,
        GssUnseqToken,
        GssGapToken,
        
        // FATAL ERROR CODES, ONLY IN CONJUNCTION WITH GssFailure
        GssBadBindings,
        GssBadMech,
        GssBadName,
        GssBadNametype,
        GssBadStatus,
        GssBadSig,
        GssBadMic,
        GssContextExpired,
        GssCredentialsExpired,
        GssDefectiveCredential,
        GssDefectiveToken,
        GssNoContext,
        GssNoCred,
        GssBadQOP,
        GssUnauthorized,
        GssUnavailable,
        GssDuplicateElement,
        GssNameNotMN
        
    }
}
