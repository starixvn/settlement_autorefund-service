namespace AutoEarlyRefund.Features.CheckSettleStatusMatch;

public class CheckSettleStatusMatchCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public string Action { get; set; } = "REFUND";
    public bool IsSettleDetermined { get; set; }
    public bool IsEarly { get; set; } = true;
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }

    public override string GetSettingKey() => "NAP_Set_SettlementStatusChecking";
    public override bool IsValid() => MatchId > 0;
}
