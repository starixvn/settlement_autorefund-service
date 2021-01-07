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

        MatchChecking GetMatchInfo(Match match);

        void UpdateStatus(Match match, int statusId);

        void UpdateRemark(Match match, string remark, int statusId);
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

        public void UpdateStatus(Match match, int statusId)
        {
            var criteria = new UpdateStatusCriteria(match, statusId);
            repository.Execute(criteria);
        }

        public void UpdateRemark(Match match, string remark, int statusId)
        {
            var criteria = new UpdateStatusCriteria(match, remark, statusId);
            repository.Execute(criteria);
        }

        public MatchChecking GetMatchInfo(Match match)
        {
            var criteria = new MatchCheckingCriteria(match);

            return repository.Get<MatchChecking>(criteria);
        }
    }
}