using AutoRefund.Constants;
using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefund.Services;

public interface IMatchService
{
    Task<List<Match>> GetMatches();

    Task<MatchChecking> GetMatchInfo(Match match);

    Task UpdateStatus(Match match, int statusId);

    Task UpdateRemark(Match match, string remark, int statusId);

    Task CheckMissedMatch();
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

    public async Task<List<Match>> GetMatches()
    {
        var matches = (await repository.FetchAsync<Match>(new MatchListingCriteria())).ToList();
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

    public async Task UpdateStatus(Match match, int statusId)
    {
        var criteria = new UpdateStatusCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            StatusId = statusId
        };
        await repository.ExecuteAsync(criteria);
    }

    public async Task UpdateRemark(Match match, string remark, int statusId)
    {
        var criteria = new UpdateStatusCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
            Remark = remark,
            StatusId = statusId
        };
        await repository.ExecuteAsync(criteria);
    }

    public async Task<MatchChecking> GetMatchInfo(Match match)
    {
        var criteria = new MatchCheckingCriteria(match);

        return await repository.GetAsync<MatchChecking>(criteria);
    }

    public async Task CheckMissedMatch()
    {
        var criteria = new UpdateStatusCriteria
        {
            StatusId = MatchStatus.Failed
        };
        await repository.ExecuteAsync(criteria);
    }
}