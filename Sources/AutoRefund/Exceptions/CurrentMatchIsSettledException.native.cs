namespace AutoRefund.Exceptions;

[Serializable]
public class CurrentMatchIsSettledException : AutoRefundException
{
    public CurrentMatchIsSettledException()
    {
        Description = "Auto refund failed: match settled.";
    }

    public CurrentMatchIsSettledException(string message)
        : base(message)
    {
    }

    public CurrentMatchIsSettledException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected CurrentMatchIsSettledException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}