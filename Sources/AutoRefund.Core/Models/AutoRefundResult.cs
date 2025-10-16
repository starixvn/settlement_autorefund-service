namespace AutoRefund.Core.Models;
public class AutoRefundResult
{
    public int AutoRefundResultId { get; set; }
    public int MatchId { get; set; }
    public int SourceId { get; set; }
    public int TypeId { get; set; }
    public int StatusId { get; set; }
    public int TryCount { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Remark { get; set; }
    public string ResourceId { get; set; }
    public string HomeValue { get; set; }
    public string AwayValue { get; set; }
    public string OtherInfo { get; set; }
}

