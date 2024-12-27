namespace AutoEarlyRefund.Features.CheckMarkedTicket;

public class CheckingMarkedTicketCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public string Action { get; set; }
    public bool IsExistMarkedTicket { get; set; }
    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"NAP_Set_CheckingMarkedTransaction_{DbServer}";
    public override bool IsValid() => true;
}
