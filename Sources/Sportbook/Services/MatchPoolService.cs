using AutoRefund.Models;
using System.Collections.Generic;
using System.Linq;

namespace AutoRefund.Services
{
    public interface IMatchPoolService
    {
        List<Match> PeekNewMatch();

        void Remove(Match match);
    }

    public class MatchPoolServiceImp : IMatchPoolService
    {
        private readonly IMatchService matchService;

        private readonly List<Match> poolOfMatches;

        private readonly object lockObject;

        public MatchPoolServiceImp(IMatchService matchService)
        {
            this.matchService = matchService;
            poolOfMatches = new List<Match>();
            lockObject = new object();
        }

        public List<Match> PoolOfMatches
        {
            get
            {
                return poolOfMatches;
            }
        }

        public List<Match> PeekNewMatch()
        {
            var matches = matchService.GetMatches();

            lock (lockObject)
            {
                var newMatches = matches.Where(x => !poolOfMatches.Any(y => y.MatchId == x.MatchId)).ToList();
                poolOfMatches.AddRange(newMatches);

                return poolOfMatches;
            }
        }

        public void Remove(Match match)
        {
            lock (lockObject)
            {
                poolOfMatches.RemoveAll(m => m.MatchId == match.MatchId);
            }
        }
    }
}