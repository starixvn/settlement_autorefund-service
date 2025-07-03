using Fanex.Data.Repository;

namespace AutoRefundFM.Features.GetSpecialSession;

public class GetSpecialSessionCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_AutoRefund_FM_ScheduleSel";
    public override bool IsValid() => true;
}
