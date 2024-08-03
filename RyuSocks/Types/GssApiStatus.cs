namespace RyuSocks.Types
{
    public enum GssApiStatus
    {
        // FATAL ERROR CODES
        GssBadBindings,
        GssBadMech,
        GssBadName,
        GssBadNametype,
        GssBadStatus,
        GssContextExpired,
        GssCredentialsExpired,
        GssDefectiveCredential,
        GssDefectiveToken,
        GssFailure,
        GssNoContext,
        GssNoCred,
        
        // INFORMATORY STATUS CODES
        GssComplete,
        GssContinueNeeded,
        GssDuplicateToken,
        GssOldToken,
        GssUnseqToken
    }
}
