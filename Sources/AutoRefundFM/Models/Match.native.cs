namespace AutoRefundFM.Models;

public class Match
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }

    public SpecialSession SpecialSession { get; set; } = new();

    public override bool Equals(object obj) => (obj is Match match)
               && match.MatchId == MatchId;

    public override int GetHashCode()
    {
        return MatchId;
    }
}
