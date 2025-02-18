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
    public async Task<List<SourcesConditions>> HandleAsync(int matchId, List<SourcesConditions> sourcesConditions)
    {
        var groupBySources = sourcesConditions
            .GroupBy(x => new
            {
                x.SportType,
                x.SourceMatchType,
                x.SourceChildMatchType,
                x.SourceResource
            }).Select(x => new
            {
                x.Key.SourceResource,
                x.Key.SportType,
                x.Key.SourceMatchType,
                x.Key.SourceChildMatchType,
                MatchId = matchId
            }).ToList();

        var json = JsonSerializer.Serialize(groupBySources, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var resources = await repository.FetchAsync<Resource>(new GetResourcesInfoCriteria(json));

        var newResources = sourcesConditions.GroupJoin(
            resources,
            sc => new { sc.SourceResource, sc.SourceMatchType, sc.SourceChildMatchType },
            s => new { s.SourceResource, s.SourceMatchType, s.SourceChildMatchType },
            (sc, s) => new SourcesConditions
            {
                Id = sc.Id,
                SportType = sc.SportType,
                ResourceId = sc.ResourceId,
                RuleId = sc.RuleId,
                RuleName = sc.RuleName,
                SourceAction = sc.SourceAction,
                MatchType = sc.MatchType,
                ChildMatchType = sc.ChildMatchType,
                SourceMatchType = sc.SourceMatchType,
                SourceChildMatchType = sc.SourceChildMatchType,
                SourceResource = sc.SourceResource,
                Conditions = sc.Conditions,

                Resource = s.FirstOrDefault()
            }).ToList();

        return newResources;
    }
}
