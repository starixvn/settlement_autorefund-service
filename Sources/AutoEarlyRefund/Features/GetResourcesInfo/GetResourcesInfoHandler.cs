using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesInfo;

public class GetResourcesInfoHandler
{
    private readonly IDynamicRepository repository;

    public GetResourcesInfoHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }
    public async Task<List<Resource>> HandleAsync(List<SourcesConditions> sourcesConditions)
    {
        var groupBySources = sourcesConditions
            .GroupBy(x => x.SourceResource)
            .ToDictionary(g => g.Key, g => g.ToList());

        var z = groupBySources.Select(x => new
        {
            SourceResource = x.Key,
            SportType = 1,
            SourceMatchType = 1,
            sourceChildMatchType = 202,
            MatchId = 83727933
        }).ToList();

        var json = JsonSerializer.Serialize(z, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var resources = await repository.FetchAsync<Resource>(new GetResourcesInfoCriteria
        {
            ResourceInfo = json
        });

        return resources.ToList();
    }
}
