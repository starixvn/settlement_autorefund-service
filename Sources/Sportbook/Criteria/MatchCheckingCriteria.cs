using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class MatchCheckingCriteria : CriteriaBase
    {
        public int MatchId { get; set; }

        public override bool IsValid()
        {
            return MatchId > 0;
        }

        public override string GetSettingKey()
        {
            return "NAP_Set_AutoRefund_ChildMatchInfo";
        }
    }
}