namespace AutoEarlyRefund.Features.GetAutoRefundResults;

public class GetAutoRefundResultsCriteria : CriteriaBase
{
    public string ResourceInfo { get; set; } = string.Empty;
    public override string GetSettingKey() => "NAP_Set_AutoRefundResult_Select";

    public override bool IsValid() => true;
}
