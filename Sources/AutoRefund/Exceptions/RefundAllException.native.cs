namespace AutoRefund.Exceptions;

[Serializable]
public class RefundAllException : AutoRefundException
{
    public RefundAllException()
    {
        Description = "Auto refund failed: match not marked to refund all.";
    }

    public RefundAllException(string message)
        : base(message)
    {
    }

    public RefundAllException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected RefundAllException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}