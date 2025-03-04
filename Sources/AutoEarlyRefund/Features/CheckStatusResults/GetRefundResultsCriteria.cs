namespace AutoEarlyRefund.Features.CheckStatusResults;

public class GetRefundResultsCriteria : CriteriaBase
{
    public string ResourceInfo { get; set; }
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceStatusInfo_Sel";
    public override bool IsValid() => true;
}
