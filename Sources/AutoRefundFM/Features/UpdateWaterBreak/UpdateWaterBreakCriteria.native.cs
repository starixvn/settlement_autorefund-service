using Fanex.Data.Repository;

namespace AutoRefundFM.Features.UpdateWaterBreak;
public class UpdateWaterBreakCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string SpecialSession { get; set; } = string.Empty;

    public override string GetSettingKey() => "NAP_Set_FastMarket_ResultDetailUpd";

    public override bool IsValid() => MatchId > 0;
}
