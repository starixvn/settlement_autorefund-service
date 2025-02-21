using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesInfo;

public class GetResourcesInfoHandler(IDynamicRepository repository)
{
    public async Task<List<SourcesConditions>> HandleAsync(int matchId, List<SourcesConditions> sourcesConditions)
    {
        var groupedSources = GetDistinctGroupedSources(sourcesConditions, matchId);

        var json = SerializeToJson(groupedSources);

        var resources = await repository.FetchAsync<Resource>(new GetResourcesInfoCriteria(json));

        var newResources = MapSourcesWithResources(sourcesConditions, resources.ToList());

        return newResources;
    }

    private static List<SourcesConditions> MapSourcesWithResources(
        List<SourcesConditions> sourcesConditions,
        List<Resource> resources)
        => sourcesConditions.GroupJoin(
            resources,
            sc => new { sc.SourceResourceId, sc.SourceMatchType, sc.SourceChildMatchType },
            s => new { SourceResourceId = s.SourceResource, s.SourceMatchType, s.SourceChildMatchType },
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
                SourceResourceId = sc.SourceResourceId,

                Resource = s.FirstOrDefault()
            }).ToList();


    private static List<ResourceInfoItemRequest> GetDistinctGroupedSources(List<SourcesConditions> sourcesConditions, int matchId)
        => sourcesConditions
            .Select(sc => new ResourceInfoItemRequest
            {
                SportType = sc.SportType,
                SourceMatchType = sc.SourceMatchType,
                SourceChildMatchType = sc.SourceChildMatchType,
                SourceResource = sc.SourceResource,
                MatchId = matchId

            })

            .Distinct()
            .ToList();


    private static string SerializeToJson(List<ResourceInfoItemRequest> groupedSources)
        => JsonSerializer.Serialize(groupedSources, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
}
