using AutoRefund.Core;
using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Caching;
using Fanex.Data.Repository;
using Mapster;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetSpecialSession;

public interface IGetSpecialSessionHandler
{
    Task<List<SpecialSession>> Handle();
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

    public async Task<List<SpecialSession>> Handle()
    {
        var results = await repository.FetchAsync<ResultDetail>(new GetSpecialSessionCriteria());

        var matches = results
             .Where(x => x.MatchId > 0)
             .Select(x => x.Adapt<SpecialSession>())
             .DistinctBy(x => new { x.MatchId, x.ResourceId })
             .ToList();


        var (hasValue, leagueId) = await cache.TryGetAsync<int>(CacheKey.LeagueId);

        var matchesToRefund = FilterMatchesBasedOnEnv(matches, hasValue ? leagueId : 0);


        return [.. matchesToRefund];
    }

    private IEnumerable<SpecialSession> FilterMatchesBasedOnEnv(IEnumerable<SpecialSession> matches, int leagueId)
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
