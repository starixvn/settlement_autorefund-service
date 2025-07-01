using AutoRefund.Core;
using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetSpecialSession;

public interface IGetSpecialSessionHandler
{
    Task<List<Match>> Handle();
}

public class GetSpecialSessionHandler : IGetSpecialSessionHandler
{
    private readonly IDynamicRepository repository;
    private readonly AppSettingsKey appSettingsKey;
    private readonly ICache cache;

    public GetSpecialSessionHandler(IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey, ICache cache)
    {
        this.repository = repository;
        this.appSettingsKey = appSettingsKey.Value;
        this.cache = cache;
    }

    public async Task<List<Match>> Handle()
    {
        var results = await repository.FetchAsync<ResultDetail>(new GetSpecialSessionCriteria());

        var matches = results
            .Where(x => x.MatchId > 0)
            .Select(x => new SpecialSession
            {
                MatchId = x.MatchId,
                LeagueId = x.LeagueId,
                ResourceId = x.ResourceId,
                StartMinute = int.Parse(x.HomeValue),
                StartSecond = int.Parse(x.OtherInfo.Split(",")[0].Split(":")[1]),
                EndMinute = int.Parse(x.AwayValue),
                EndSecond = int.Parse(x.OtherInfo.Split(",")[1].Split(":")[1]),
                ResultStatusId = x.ResultStatusId,
                OtherInfo = x.OtherInfo
            })
            .GroupBy(x => new
            {
                x.MatchId,
                x.LeagueId,
            })
            .Select(x => new Match
            {
                MatchId = x.Key.MatchId,
                LeagueId = x.Key.LeagueId,
                SpecialSessions = [.. x]
            }).ToList();


        var (hasValue, leagueId) = await cache.TryGetAsync<int>(CacheKey.LeagueId);

        var matchesToRefund = FilterMatchesBasedOnEnv(matches, hasValue ? leagueId : 0);


        return matchesToRefund.ToList();
    }

    private IEnumerable<Match> FilterMatchesBasedOnEnv(IEnumerable<Match> matches, int leagueId)
    {
        return appSettingsKey.EnvironmentName switch
        {
            "PRO" => leagueId > 0
                ? matches.Where(m => m.LeagueId != leagueId)
                : matches,
            "UAT" => matches.Where(m => m.LeagueId == leagueId),
            _ => matches
        };
    }
}
