namespace AutoRefund.Core.Exceptions;

public class AutoRefundException : Exception
{
    public string Description { get; set; } = string.Empty;
    public bool StopLoop { get; set; }
}
