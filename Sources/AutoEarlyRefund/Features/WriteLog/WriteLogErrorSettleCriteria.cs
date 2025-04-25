namespace AutoEarlyRefund.Features.WriteLog;

public class WriteLogErrorSettleCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public int UserId { get; set; }
    public string Remark { get; set; } = string.Empty;
    public int ActionId { get; set; } = 81022; // RefundSingleAction
    public string BroadcastSessionId { get; set; } = Guid.NewGuid().ToString();

    public override string GetSettingKey() => "WriteLog_UserLog_Settle";
    public override bool IsValid() => MatchId > 0;
}
