namespace AutoEarlyRefund.Features.PeekNewMatches;

public class GetResultsCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_AutoRefund_Resource_ScheduleSel";

    public override bool IsValid() => true;
}