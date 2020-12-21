using AutoRefund.Exceptions;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class LogCriteria : NonQueryCommand
    {
        public LogCriteria(Match match, AutoRefundException exception)
        {
            MatchId = match.MatchId;
            Remark = exception.Description ?? string.Empty;
        }

        public LogCriteria(Match match, string errorMessage)
        {
            MatchId = match.MatchId;
            Remark = errorMessage;
        }

        public int UserId { get; }

        public int MatchId { get; set; }

        public string Remark { get; set; }

        public string IP { get; } = string.Empty;

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