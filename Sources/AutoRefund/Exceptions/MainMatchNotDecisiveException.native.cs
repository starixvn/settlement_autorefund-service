using System.Runtime.Serialization;

namespace AutoRefund.Exceptions;

[Serializable]
public class MainMatchNotDecisiveException : AutoRefundException
{
    public MainMatchNotDecisiveException()
    {
        Description = "Auto refund failed: main match is draw.";
    }

    public MainMatchNotDecisiveException(string message)
        : base(message)
    {
    }

    public MainMatchNotDecisiveException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected MainMatchNotDecisiveException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}