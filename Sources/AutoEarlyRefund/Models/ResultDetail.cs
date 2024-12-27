namespace AutoEarlyRefund.Models;
public class ResultDetail
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public string ResourceId { get; set; }
    public string HomeValue { get; set; }
    public string AwayValue { get; set; }
    public DateTime LastModifyDate { get; set; }
}
