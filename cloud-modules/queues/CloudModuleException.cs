namespace queues;

public enum ErrorCodes {

    SUCCESS                    =  0,
    UNKNOWN_FAILURE            = -1,
    NO_MESSAGES_FOUND_IN_QUEUE = -1002,
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