using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoRefundFM.Features.CheckingMarkedTicket;

public class CheckingMarkedTicketCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public string Results { get; set; } = string.Empty;

    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public override string GetSettingKey() => $"NAP_Set_FastMarket_CheckingMarkedTransaction_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}