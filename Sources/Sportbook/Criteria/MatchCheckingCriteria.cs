using AutoRefund.Constants;
using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class MatchCheckingCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int TypeId { get; } = MatchType.AutoRefund;

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        public override bool IsValid()
        {
            return MatchId > 0;
        }

        public override string GetSettingKey()
        {
            return "NAP_Set_AutoRefund_MatchChecking";
        }
    }
}