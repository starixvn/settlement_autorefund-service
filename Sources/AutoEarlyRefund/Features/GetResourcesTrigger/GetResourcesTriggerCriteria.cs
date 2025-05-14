namespace AutoEarlyRefund.Features.GetResourcesTrigger;

public class GetResourcesTriggerCriteria : CriteriaBase
{
    public short SportType { get; set; }
    public short ChildMatchType { get; set; }
    public string Action { get; set; }
    public string ResourceIds { get; set; }
    public override string GetSettingKey() => "NAP_Set_AutoRefund_ResourceTrigger_Sel";

    public override bool IsValid() => true;
}
