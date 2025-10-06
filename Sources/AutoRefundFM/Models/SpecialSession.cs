namespace AutoRefundFM.Models;

public class SpecialSession
{
    private const int startTimeIndex = 0;
    private const int endTimeIndex = 1;
    private const int secondIndex = 1;

    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string HomeValue { get; set; } = string.Empty;
    public string AwayValue { get; set; } = string.Empty;
    public int StartMinute => int.Parse(HomeValue);
    public int StartSecond => int.Parse(OtherInfo.Split(",")[startTimeIndex].Split(":")[secondIndex]);
    public int EndMinute => int.Parse(AwayValue);
    public int EndSecond => int.Parse(OtherInfo.Split(",")[endTimeIndex].Split(":")[secondIndex]);
    public int ResultStatusId { get; set; }
    public string OtherInfo { get; set; } = string.Empty;
    public List<FastMarketResultDetail> FastMarketResultDetails { get; set; } = [];
    public List<EventCounter> EventCounters { get; set; } = [];

    public DateTimeOffset FirstProcess { get; set; } = DateTimeOffset.Now;

    public override string ToString() => $"{ResourceId}({OtherInfo})";

    public override bool Equals(object? obj) => (obj is SpecialSession session)
        && session.MatchId == MatchId
        && session.ResourceId == ResourceId;

    public override int GetHashCode() => HashCode.Combine(MatchId, ResourceId);
}
