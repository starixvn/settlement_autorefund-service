namespace AutoEarlyRefund.Features.GetRefundMatchInfoSel;

public class GetRefundMatchInfoCriteria : CriteriaBase
{
    public string MatchInfo { get; set; } = string.Empty;
    public override string GetSettingKey() => "NAP_Set_AutoRefund_MatchInfo_Sel";

    public override bool IsValid() => true;
}
