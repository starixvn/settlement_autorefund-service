namespace AutoRefundFM.Models;

public class Match
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }

    public List<SpecialSession> SpecialSessions { get; set; } = [];

    public override bool Equals(object obj) => (obj is Match match)
               && match.MatchId == MatchId;

    public override int GetHashCode()
    {
        return MatchId;
    }
}
