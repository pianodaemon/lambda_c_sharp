namespace CloudModules;

public enum ErrorCodes {

    SUCCESS                    =  0,
    UNKNOWN_FAILURE            = -1,
    NO_MESSAGES_FOUND_IN_QUEUE = -1002,
    JSON_MESSAGE_IS_NULL       = -1003,
    BUCKET_IS_NOT_SET          = -1004,
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