using Fanex.Data.Repository;

namespace AutoRefundFM.Features.GetFastMarketResults;

public class GetFastMarketResultsCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey() => "NAP_Set_FastMarket_MatchSelDetail";

    public override bool IsValid() => MatchId > 0;
}
