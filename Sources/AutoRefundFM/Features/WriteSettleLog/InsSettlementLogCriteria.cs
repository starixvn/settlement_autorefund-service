using Fanex.Data.Repository;

namespace AutoRefundFM.Features.WriteSettleLog;

public class InsSettlementLogCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Action { get; set; } = "REFUND";

    public string Mode { get; set; } = "FM";

    public int Sporttype { get; set; } = 1; //SportType for Soccer

    public bool IsAuto { get; set; } = true;

    public int AdminId { get; set; }

    public string SelectedResources { get; set; } = string.Empty;
    public override string GetSettingKey() => "NAP_SettlementLog_Ins";

    public override bool IsValid() => true;
}
