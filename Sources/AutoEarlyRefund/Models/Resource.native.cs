using System.Text.Json;

namespace AutoEarlyRefund.Models;

public class Resource
{
    public int MatchId { get; set; }
    public int ParentId { get; set; }
    public short SportType { get; set; }
    public int ResultTable { get; set; }
    public string SourceResource { get; set; } = string.Empty;
    public short SourceMatchType { get; set; }
    public short SourceChildMatchType { get; set; }
    public string HomeValue { get; set; } = string.Empty;
    public string AwayValue { get; set; } = string.Empty;
    public string CfmHomeValue { get; set; } = string.Empty;
    public string CfmAwayValue { get; set; } = string.Empty;
    public short ResultStatusId { get; set; }
    public short SettlementStatusId { get; set; }

    public byte ResultSetting { get; set; }
    public bool IsConfirmedRefundSdt => (ResultSetting & (1 << 2)) != 0;
    public bool IsConfirmedRefund => (ResultSetting & (1 << 1)) != 0;

    public override string ToString()
        => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}
