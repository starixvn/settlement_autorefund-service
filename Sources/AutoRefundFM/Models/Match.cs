namespace AutoRefundFM.Models;

public class Match
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }

    public List<SpecialSession> SpecialSessions { get; set; }
}
