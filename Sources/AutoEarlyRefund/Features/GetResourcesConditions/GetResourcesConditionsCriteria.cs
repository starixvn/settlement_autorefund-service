namespace AutoEarlyRefund.Features.GetResourcesConditions;
public class GetResourcesConditionsCriteria : CriteriaBase
{
    public string ResourceInfo { get; set; }
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceConditions_Sel";

    public override bool IsValid() => true;
}

