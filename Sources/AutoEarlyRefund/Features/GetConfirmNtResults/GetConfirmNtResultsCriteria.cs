namespace AutoEarlyRefund.Features.GetConfirmNtResults;

public class GetConfirmNtResultsCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceCfm_ScheduleSel";

    public override bool IsValid() => true;
}
