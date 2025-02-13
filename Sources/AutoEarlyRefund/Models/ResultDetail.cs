namespace AutoEarlyRefund.Models;
public class ResultDetail
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public string SourceId { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string HomeValue { get; set; } = string.Empty;
    public string AwayValue { get; set; } = string.Empty;
    public DateTime LastModifyDate { get; set; }
    public string Remark { get; set; } = string.Empty;
    public byte ResultStatusId { get; set; }
    public short SettlementStatusId { get; set; }

    public byte ResultSetting { get; set; }
    public bool IsConfirmedRefund { get; set; }
    public bool IsConfirmedRefundSdt { get; set; }

    public override string ToString() => $"{ResourceId}({HomeValue}-{AwayValue})";
}
