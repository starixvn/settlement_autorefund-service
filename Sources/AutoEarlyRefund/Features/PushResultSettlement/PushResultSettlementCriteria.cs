namespace AutoEarlyRefund.Features.PushResultSettlement;

public class PushResultSettlementCriteria : NonQueryCommand
{
    public int AdminId { get; set; }

    public int MatchId { get; set; }

    public string SelectedResources { get; set; }

    public string Action { get; set; }
    public bool IsAuto { get; set; }

    public string BroadcastSessionId { get; } = Guid.NewGuid().ToString();

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    [SpParam(Ignored = true)]
    public string SpName => "NAP_Admin_Settlement_Auto_1s_Push";

    public override string GetSettingKey() => $"NAP_Admin_Settlement_Auto_1s_Push_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}
