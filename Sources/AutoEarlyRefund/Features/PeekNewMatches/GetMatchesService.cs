using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;
using AutoRefund.Core;

namespace AutoEarlyRefund.Features.PeekNewMatches;

public interface GetMatchesService
{
    Task<List<Match>> GetMatches();
}

public class GetMatchesServiceImp : GetMatchesService
{
    private readonly IDynamicRepository repository;
    private readonly ICache cache;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly XmlConvertService xmlConvertService;

    public GetMatchesServiceImp(
        IDynamicRepository repository,
        ICache cache,
        IOptions<AppSettingsKey> appSettingsKey,
        XmlConvertService xmlConvertService)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.appSettingsKey = appSettingsKey ?? throw new ArgumentNullException(nameof(appSettingsKey));
        this.xmlConvertService = xmlConvertService ?? throw new ArgumentNullException(nameof(xmlConvertService));
    }

    public async Task<List<Match>> GetMatches()
    {
        var resultDetails = await repository.FetchAsync<ResultDetail>(new GetPendingRefundResultsCriteria());

        if (resultDetails == null || !resultDetails.Any())
        {
            return new List<Match>();
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