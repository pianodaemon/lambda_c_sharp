namespace queues;

public enum ErrorCodes {

    SUCCESS = 0,
    UNKNOWN_FAILURE = -1
}

public class QueueException : Exception
{
    public QueueException(string msg, Exception cause) : base(msg, cause)
    {

    }

    public QueueException(String msg, Exception cause, ErrorCodes errorCode) : this(msg, cause)
    {
        this.ErrorCode = errorCode;
    }

    public QueueException(String msg, ErrorCodes errorCode) : base(msg)
    {
        this.ErrorCode = errorCode;
    }

    public ErrorCodes ErrorCode { get; }
}