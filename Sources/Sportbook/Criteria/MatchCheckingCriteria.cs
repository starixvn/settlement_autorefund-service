using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class MatchCheckingCriteria : NonQueryCommand
    {

        public MatchCheckingCriteria(Match match)
        {
            MatchId = match.MatchId;
            TypeId = match.TypeId;
        }

        public int MatchId { get; set; }

        public int TypeId { get; set; }

        public override bool IsValid()
        {
            return MatchId > 0;
        }

        public override string GetSettingKey()
        {
            return "NAP_Set_AutoRefund_MatchInfo";
        }
    }
}