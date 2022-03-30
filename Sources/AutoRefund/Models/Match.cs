namespace AutoRefund.Models
{
    public class Match
    {
        public int MatchId { get; set; }

        public int LeagueId { get; set; }

        public bool IsChecking { get; set; }

        public int TypeId { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null
                   && (obj is Match match)
                   && match.MatchId == MatchId;
        }

        public override int GetHashCode()
        {
            return MatchId;
        }
    }
}