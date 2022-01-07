using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
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

        void CheckMissedMatch();
    }

    public class MatchServiceImp : IMatchService
    {
        private readonly IDynamicRepository repository;

        private const string KEY_LEAGUEID = "Settlement-AutoPayout-AutoInstant-LeagueId";

        private readonly ICache cache;

        private readonly AppSettingsKey appSettingsKey;

        public MatchServiceImp(IDynamicRepository repository,
            ICache cache,
            IOptions<AppSettingsKey> appSettingsKey)
        {
            this.repository = repository;
            this.cache = cache;
            this.appSettingsKey = appSettingsKey.Value;
        }

        public List<Match> GetMatches()
        {
            var matches = repository.Fetch<Match>(new MatchListingCriteria()).ToList();
            cache.TryGet(KEY_LEAGUEID, out int leagueId);

            switch (appSettingsKey.EnvironmentName)
            {
                case "PRO":
                    return leagueId > 0
                        ? (from m in matches where m.LeagueId != leagueId select m).ToList()
                        : matches;
                case "UAT":
                    return (from m in matches where m.LeagueId == leagueId select m).ToList();
                default:
                    return matches;
            }
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

        public void CheckMissedMatch()
        {
            var criteria = new UpdateStatusCriteria();
            repository.Execute(criteria);
        }
    }
}