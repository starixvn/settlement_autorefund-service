namespace AutoRefundFM;

public class AutoRefundFMException : Exception
{
    public AutoRefundFMException(string message) : base(message)
    {
    }
    public AutoRefundFMException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
