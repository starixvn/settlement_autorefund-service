using AutoRefund.Constants;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class LogCriteria : NonQueryCommand
    {
        public LogCriteria(Match match, string errorMessage, int TypeID)
        {
            MatchId = match.MatchId;
            Remark = errorMessage;
            ActionId = TypeID == MatchType.AutoRefund ? MatchType.AutoRefundSingle : MatchType.AutoRefundParlay;
        }

        public int UserId { get; }

        public int MatchId { get; set; }

        public string Remark { get; set; }

        public string IP { get; } = string.Empty;

        public int ActionId { get; }

        public override bool IsValid()
        {
            return MatchId > 0;
        }

        public override string GetSettingKey()
        {
            return "WriteLog_UserLog_Settle";
        }
    }
}