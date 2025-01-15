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
    XmlConvertService xmlConvertService) : GetMatchesService
{
    public async Task<List<Match>> GetMatches()
    {
        var resultDetails = await repository.FetchAsync<ResultDetail>(new GetResultsCriteria());

        var matches = resultDetails
            .GroupBy(x => new { x.MatchId, x.LeagueId })
            .Select(group => new Match
            {
                MatchId = group.Key.MatchId,
                LeagueId = group.Key.LeagueId,
                ResultDetails = group.ToList()
            })
            .Select(x => x.WithResourcesXml(xmlConvertService));

        var (_, leagueId) = await cache.TryGetAsync<int>(CacheKey.LeagueId);

        var matchesToRefund = FilterMatchesBasedOnEnv(matches, leagueId);

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