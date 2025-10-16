using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoRefundFM.Features.GetFastMarketResults;
public class GetFastMarketTicketDetailCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public override string GetSettingKey() => $"NAP_Set_FastMarket_MatchSelDetail_Ticket_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}