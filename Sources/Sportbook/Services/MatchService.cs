using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;

namespace AutoRefund.Services
{
    public interface IMatchService
    {
        List<Match> GetMatches();

        void UpdateStatus(Match match);
    }

    public class MatchServiceImp : IMatchService
    {
        private readonly IDynamicRepository repository;

        public MatchServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public List<Match> GetMatches()
        {
            var matches = repository.Fetch<Match>(new MatchListingCriteria()).ToList();
            var leagueSetting = LeagueSettingFactory.Create();

            if (leagueSetting.ExcludeLeagueIds.Any())
            {
                matches = (from m in matches
                           where !leagueSetting.ExcludeLeagueIds.Any(id => m.LeagueId.Equals(id))
                           select m).ToList();
            }

            if (leagueSetting.IncludeLeagueIds.Any())
            {
                matches = (from m in matches
                           where leagueSetting.IncludeLeagueIds.Any(id => m.LeagueId.Equals(id))
                           select m).ToList();
            }

            return matches;
        }

        public void UpdateStatus(Match match)
        {
            var criteria = new UpdateStatusCriteria { MatchId = match.MatchId };
            repository.Execute(criteria);
        }
    }
}