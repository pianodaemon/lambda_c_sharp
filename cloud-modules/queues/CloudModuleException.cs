namespace CloudModules;

public enum ErrorCodes {

    SUCCESS                    =  0,
    UNKNOWN_FAILURE            = -1,
    NO_MESSAGES_FOUND_IN_QUEUE = -1002,
    JSON_MESSAGE_IS_NULL       = -1003,
    JSON_MESSAGE_WAS_NOT_DES   = -1004,
    BUCKET_IS_NOT_SET          = -2005,
    BUCKET_CANNOT_LIST_OBJECTS = -2006,
    SECRET_FAILURE_VAL_RES     = -3001
}

public class CloudModuleException : Exception
{
    public CloudModuleException(string msg, Exception cause) : base(msg, cause)
    {

    }

    public CloudModuleException(String msg, Exception cause, ErrorCodes errorCode) : this(msg, cause)
    {
        this.ErrorCode = errorCode;
    }

    public CloudModuleException(String msg, ErrorCodes errorCode) : base(msg)
    {
        this.ErrorCode = errorCode;
    }

    public ErrorCodes ErrorCode { get; }
}
