namespace AutoRefund.Models
{
    public class Match
    {
        public int MatchId { get; set; }

        public int LeagueId { get; set; }

        public int TypeId { get; set; }

        public bool IsChecking { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is Match match)
                   && match.MatchId == MatchId
                   && match.TypeId == TypeId;
        }

        public override int GetHashCode()
        {
            return MatchId * TypeId;
        }
    }
}