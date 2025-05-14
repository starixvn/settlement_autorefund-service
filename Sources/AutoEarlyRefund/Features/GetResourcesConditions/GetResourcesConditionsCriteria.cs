namespace AutoEarlyRefund.Features.GetResourcesConditions;
public class GetResourcesConditionsCriteria(string json) : CriteriaBase
{
    public string ResourceInfo { get; set; } = json;
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceConditions_Sel";

    public override bool IsValid() => true;
}

