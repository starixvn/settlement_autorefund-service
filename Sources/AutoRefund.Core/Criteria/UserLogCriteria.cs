using Fanex.Data.Repository;

namespace AutoRefund.Core.Criteria;

public class UserLogCriteria : NonQueryCommand
{
    public int UserId { get; set; }

    public int MatchId { get; set; }

    public string Remark { get; set; }

    public string Ip { get; } = string.Empty;

    public int ActionId { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "WriteLog_UserLog_Settle";
}

