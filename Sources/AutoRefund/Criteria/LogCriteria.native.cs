namespace AutoRefund.Criteria;

using Models;

public class LogCriteria : NonQueryCommand
{
    private const int RefundSingleAction = 81022;

    public LogCriteria(Match match, string errorMessage)
    {
        MatchId = match.MatchId;
        Remark = errorMessage;
        ActionId = RefundSingleAction;
    }

    public int UserId { get; set; }

    public int MatchId { get; set; }

    public string Remark { get; set; }

    public string IP { get; set; } = string.Empty;

    public int ActionId { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "WriteLog_UserLog_Settle";
}