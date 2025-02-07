namespace AutoEarlyRefund.Models;

public class Resource
{
    public int MatchId { get; set; }
    public int ParentId { get; set; }
    public short SportType { get; set; }
    public int ResultTable { get; set; }
    public string SourceResource { get; set; } = string.Empty;
    public int SourceMatchType { get; set; }
    public int SourceChildMatchType { get; set; }
    public string HomeValue { get; set; } = string.Empty;
    public string AwayValue { get; set; } = string.Empty;
    public short ResultStatusId { get; set; }
    public short SettlementStatusId { get; set; }
    public bool IsConfirmedRefundSdt { get; set; }
    public bool IsConfirmedRefund { get; set; }
}
