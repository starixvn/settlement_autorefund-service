namespace AutoEarlyRefund.Features.GetResourcesInfo;

public class GetSrcResourcesCriteria : CriteriaBase
{
    public GetSrcResourcesCriteria(string resourceInfo)
    {
        ResourceInfo = resourceInfo ?? string.Empty;
    }

    public string ResourceInfo { get; set; }
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceInfo_Sel";

    public override bool IsValid() => true;
}
