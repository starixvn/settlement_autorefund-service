using AutoRefundFM.Models;
namespace AutoRefundFM;

public class AutoRefundFMException : Exception
{
    public Match Match { set; get; } = new();

    public AutoRefundFMException(string message) : base(message)
    {
    }
    public AutoRefundFMException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AutoRefundFMException(string message, Match match) : base(message)
    {
        Match = match;
    }
}
