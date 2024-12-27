using AutoEarlyRefund.Models;
using AutoRefund.Core;
using Fanex.Caching;
using Fanex.Settlement.Converter.Services;

namespace AutoEarlyRefund.Features.FetchItems;

public class FetchItemsHandler
{
    private readonly IDynamicRepository repository;
    private readonly ICache cache;
    private IOptions<AppSettingsKey> appSettingsKey;
    private readonly XmlConvertService xmlConvertService;

    public FetchItemsHandler(IDynamicRepository repository, ICache cache, IOptions<AppSettingsKey> appSettingsKey, XmlConvertService xmlConvertService)
    {
        this.repository = repository;
        this.cache = cache;
        this.appSettingsKey = appSettingsKey;
        this.xmlConvertService = xmlConvertService;
    }

    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var resultDetails = await repository.FetchAsync<ResultDetail>(new FetchItemsCriteria(), cancellationToken: cancellationToken);

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

        var matchesToRefund = FilterMatchesBasedOnEnvironment(matches, leagueId);
    }


    private IEnumerable<Match> FilterMatchesBasedOnEnvironment(IEnumerable<Match> matches, int leagueId)
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
