using AutoEarlyRefund.Models;
using AutoRefund.Core;

namespace AutoEarlyRefund.Features.PeekNewMatches;

public interface GetMatchesService
{
    Task<List<Match>> GetMatches();
}

public class GetMatchesServiceImp(
    IDynamicRepository repository,
    ICache cache,
    IOptions<AppSettingsKey> appSettingsKey,
    XmlConvertService xmlConvertService)
    : GetMatchesService
{
    public async Task<List<Match>> GetMatches()
    {
        var resultDetails = await repository.FetchAsync<ResultDetail>(new GetPendingRefundResultsCriteria());

        if (resultDetails == null || !resultDetails.Any())
        {
            return [];
        }

        var matches = resultDetails
            .GroupBy(x => new { x.MatchId, x.LeagueId, x.SportType, x.ChildMatchType })
            .Select(group => new Match
            {
                MatchId = group.Key.MatchId,
                LeagueId = group.Key.LeagueId,
                SportType = group.Key.SportType,
                ChildMatchType = group.Key.ChildMatchType,
                ResultDetails = group.ToList()
            })
            .Select(x => x.WithResourcesXml(xmlConvertService));

        var (hasValue, leagueId) = await cache.TryGetAsync<int>(CacheKey.LeagueId);

        var matchesToRefund = FilterMatchesBasedOnEnv(matches, hasValue ? leagueId : 0);

        return matchesToRefund.ToList();
    }

    private IEnumerable<Match> FilterMatchesBasedOnEnv(IEnumerable<Match> matches, int leagueId)
    {
        return appSettingsKey.Value.EnvironmentName switch
        {
            "PRO" => leagueId > 0
                ? matches.Where(m => m.LeagueId != leagueId)
                : matches,
            "UAT" => matches.Where(m => m.LeagueId == leagueId),
            _ => matches
        };
    }
}