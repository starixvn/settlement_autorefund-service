using Fanex.Data.Repository;

namespace AutoRefund.Criteria;

public class MatchListingCriteria : CriteriaBase
{
    public override bool IsValid() => true;

    public override string GetSettingKey() => "NAP_Set_AutoRefund_ScheduleSel";
}