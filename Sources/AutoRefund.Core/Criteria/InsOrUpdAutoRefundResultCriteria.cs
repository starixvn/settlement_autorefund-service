using Fanex.Data.Repository;

namespace AutoRefund.Core.Criteria;
public class InsOrUpdAutoRefundResultCriteria : INonQueryCommand
{
    public int MatchId { get; set; }
    public int StatusId { get; set; }
    public string SelectedResources { get; set; } = string.Empty;
    public int TypeId { get; set; } = 11; //Auto  Refund FM

    public string GetSettingKey() => "NAP_Set_AutoRefundResult_Update";

    public bool IsValid() => MatchId > 0;
}