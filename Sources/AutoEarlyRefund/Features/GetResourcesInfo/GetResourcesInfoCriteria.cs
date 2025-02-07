namespace AutoEarlyRefund.Features.GetResourcesInfo;

public class GetResourcesInfoCriteria : CriteriaBase
{
    public string ResourceInfo { get; set; } = string.Empty;
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceInfo_Sel";

    public override bool IsValid() => true;
}
