namespace AutoRefundFM.Models;
public class TicketCounts
{
    private const int HalftimeTitle = 1;

    private const int FullTimeTitle = 2;

    private const int EarlySettleTitle = 8;

    private const int FastMarketTitle = 9;

    public int GroupId { get; set; }

    public string GroupName
    {
        get
        {
            return GroupId switch
            {
                HalftimeTitle => "HT Tickets",
                FullTimeTitle => "FT Tickets",
                EarlySettleTitle => "Early Settle Tickets",
                FastMarketTitle => "Fast Market Tickets",
                _ => "Total Tickets"
            };
        }
    }

    public int BetCount { get; set; }

    public int BetRunningCount { get; set; }

    public int BetSettledCount { get; set; }

    public int BetMarkedCount { get; set; }

    public int BetRefundedCount { get; set; }

    public int BetWaitingCount { get; set; }
}