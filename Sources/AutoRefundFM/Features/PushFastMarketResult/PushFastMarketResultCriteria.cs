using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoRefundFM.Features.PushFastMarketResult;
public class PushFastMarketResultCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Results { get; set; } = string.Empty;

    public string EventCounters { get; set; } = string.Empty;

    public bool IsSettleDetermined { get; set; }

    public string Action { get; set; } = "REFUND";

    public bool IsAuto { get; set; } = true;

    public bool IsReSettledTickets { get; set; }

    public string Ip { get; set; }

    public string OldResults { get; set; }

    public bool IsOrg { get; set; } = true;

    public string BroadcastSessionId { get; } = Guid.NewGuid().ToString();

    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    [SpParam(Ignored = true)]
    public string SpName => "NAP_Admin_Settlement_Fast_Market_Auto_1s_Push";

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = string.Empty;

    public override string GetSettingKey() => $"NAP_Admin_Settlement_Fast_Market_Auto_1s_Push_{DbServer}";

    public override bool IsValid() => AdminId > 0 && MatchId > 0;
}