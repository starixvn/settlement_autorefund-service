namespace AutoRefundFM.Models;

public class Match
{
    public int MatchId { get; set; }

    public List<SpecialSession> SpecialSessions { get; set; }
}
