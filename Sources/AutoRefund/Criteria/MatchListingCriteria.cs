using Fanex.Data.Repository;

namespace AutoRefund.Criteria
{
    public class MatchListingCriteria : CriteriaBase
    {
        public override bool IsValid()
        {
            return true;
        }

        public override string GetSettingKey()
        {
            return "NAP_Set_AutoRefund_ScheduleSel";
        }
    }
}