namespace AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;

public class InsOrUpdAutoRefundResultCriteria : INonQueryCommand
{
    public int MatchId { get; set; }
    public int StatusId { get; set; }
    public string SelectedResources { get; set; }
    public int TypeId { get; set; } = 7; //Direct early soccer
    public string Remark { get; set; }
    public string GetSettingKey() => "NAP_Set_AutoRefundResult_Update";

    public bool IsValid() => MatchId > 0;
}
