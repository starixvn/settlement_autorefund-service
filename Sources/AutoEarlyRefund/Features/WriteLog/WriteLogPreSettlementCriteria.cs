namespace AutoEarlyRefund.Features.WriteLog;

public class WriteLogPreSettlementCriteria : NonQueryCommand
{
    public int UserId { get; set; }
    public string Doing { get; set; }
    public int Extend { get; set; }

    public override string GetSettingKey() => "NAP_UserLog_WriteLogPreSettlement";
    public override bool IsValid() => true;
}
