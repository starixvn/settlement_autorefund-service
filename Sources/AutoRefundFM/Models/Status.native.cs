namespace AutoRefundFM.Models;

public static class Status
{
    public const int RunningTicket = 2;

    public const int CalculatedTicket = 3;

    public const int NoTicket = 4;

    public const int MarkedTicket = 5;

    public const int RunningResult = 3;

    public const int CompletedAutoResult = 4;

    public const int CompletedManualResult = 5;

    public const int RefundResult = 6;

    public const int RefundDeterminedResult = 7;

    private static Dictionary<int, string> TicketStatusName => new()
    {
        { RunningTicket, TicketStatus.Running }, { CalculatedTicket, TicketStatus.Calculated }, { NoTicket, TicketStatus.NoTicket }, { MarkedTicket, TicketStatus.Marked }
    };

    public static Dictionary<int, string> ResultStatus => new()
    {
        { RunningResult, "Running" }, { CompletedAutoResult, "Completed (Auto)" }, { CompletedManualResult,"Completed (Manual)" },
        { RefundResult, "Refund" }, { RefundDeterminedResult, "Refund SDT" }
    };

    public static string GetTicketStatusNameById(int id)
    {
        return TicketStatusName.FirstOrDefault(x => x.Key == id).Value;
    }

    public static string GetResultStatusNameById(int id)
    {
        var running = new List<int> { 0, 1, 2, 3 };

        return running.Contains(id) ? "Running" : ResultStatus.FirstOrDefault(x => x.Key == id).Value;
    }
}