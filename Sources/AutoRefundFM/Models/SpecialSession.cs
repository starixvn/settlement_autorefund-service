namespace AutoRefundFM.Models;

public class SpecialSession
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public int StartSecond { get; set; }
    public int StartMinute { get; set; }
    public int EndSecond { get; set; }
    public int EndMinute { get; set; }
    public int ResultStatusId { get; set; }
    public string OtherInfo { get; set; }
}
