namespace AutoRefund.Criteria;

public class RefundPushCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Action { get; set; } = "REFUND";

    public int HalfTimeHome { get; set; } = 0;

    public int HalfTimeAway { get; set; } = 0;

    public int FullTimeHome { get; set; } = 0;

    public int FullTimeAway { get; set; } = 0;

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public int IsAuto { get; set; } = 1;

    public string BroadcastSessionId { get; set; } = Guid.NewGuid().ToString();

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"NAP_Admin_Settlement_Refund_1_Push_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}