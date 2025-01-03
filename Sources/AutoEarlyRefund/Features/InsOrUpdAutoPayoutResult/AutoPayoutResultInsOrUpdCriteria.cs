namespace AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;

public class AutoPayoutResultInsOrUpdCriteria : INonQueryCommand
{
    public int MatchId { get; set; }
    public int StatusId { get; set; }
    public string SelectedResources { get; set; }
    public int CalculationType { get; set; } = 5; //Direct early soccer
    public string Remark { get; set; }

    public string GetSettingKey() => "NAP_Set_AutoPayoutResult_Update";

    public bool IsValid() => MatchId > 0;
}