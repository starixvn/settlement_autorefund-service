namespace AutoRefund.Models
{
    public class Match
    {
        public int MatchId { get; set; }

        public int LeagueId { get; set; }

        public int finalhomescore { get; set; }

        public int finalawayscore { get; set; }

        public int NumberOfRetries { get; set; }
    }
}