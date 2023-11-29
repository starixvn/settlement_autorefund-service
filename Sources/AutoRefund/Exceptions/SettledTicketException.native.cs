namespace AutoRefund.Exceptions;

[Serializable]
public class SettledTicketException : AutoRefundException
{
    public SettledTicketException()
    {
        Description = "Auto refund failed: settled tickets exist.";
    }

    public SettledTicketException(string message)
        : base(message)
    {
    }

    public SettledTicketException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    protected SettledTicketException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}