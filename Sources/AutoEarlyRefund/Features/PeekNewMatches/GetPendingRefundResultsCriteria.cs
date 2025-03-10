namespace AutoEarlyRefund.Features.PeekNewMatches;

public class GetPendingRefundResultsCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_AutoRefund_Resource_ScheduleSel";

    public override bool IsValid() => true;
}