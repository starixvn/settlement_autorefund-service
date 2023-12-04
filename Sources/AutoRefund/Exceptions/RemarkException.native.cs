namespace AutoRefund.Exceptions;

[Serializable]
public class RemarkException : AutoRefundException
{
    public RemarkException()
    {
        Description = "Auto refund failed: match in on hold.";
    }

    public RemarkException(string message)
        : base(message)
    {
    }

    public RemarkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected RemarkException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}