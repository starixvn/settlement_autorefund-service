namespace AutoEarlyRefund.Models;

public class SourcesConditions
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public short ChildMatchType { get; set; }
    public short MatchType { get; set; }
    public string RuleName { get; set; } = string.Empty;

    public string SourceAction { get; set; }
    public short SourceMatchType { get; set; }
    public short SourceChildMatchType { get; set; }
    public string SourceResource { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;

    //Source Resource Info
    #region Resource Details
    public int MatchId { get; set; }
    public int ParentId { get; set; }
    public int ResultTable { get; set; }
    public string HomeValue { get; set; } = string.Empty;
    public string AwayValue { get; set; } = string.Empty;
    public short ResultStatusId { get; set; }
    public short SettlementStatusId { get; set; }
    public bool IsConfirmedRefundSdt { get; set; }
    public bool IsConfirmedRefund { get; set; }
    #endregion

    public override string ToString() => $"{ResourceId}{RuleName}, Target: {SourceResource}({HomeValue}-{AwayValue})";
}
