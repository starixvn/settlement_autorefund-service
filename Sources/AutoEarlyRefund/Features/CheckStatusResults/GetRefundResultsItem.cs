namespace AutoEarlyRefund.Features.CheckStatusResults;

public class GetRefundResultsItem
{
    public int MatchId { get; set; }
    public int SportType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
}
