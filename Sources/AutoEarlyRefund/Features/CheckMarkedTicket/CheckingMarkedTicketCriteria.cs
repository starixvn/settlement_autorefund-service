namespace AutoEarlyRefund.Features.CheckMarkedTicket;

public class CheckingMarkedTicketCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public string ResourceIds { get; set; } = string.Empty;
    public bool IsExistMarkedTicket { get; set; }
    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public int ErrCode { get; set; }
    public string ErrMsg { get; set; } = string.Empty;

    public override string GetSettingKey() => $"NAP_Set_EarlySettlement_CheckingMarkedTransaction_{DbServer}";
    public override bool IsValid() => MatchId > 0;
}
