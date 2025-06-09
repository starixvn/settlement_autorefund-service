namespace AutoEarlyRefund.Features.GetResultDetails;

public class GetResultDetailsCriteria : CriteriaBase
{
    public string ResourceInfo { get; set; } = string.Empty;

    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceStatusInfo_Sel";

    public override bool IsValid() => true;
}