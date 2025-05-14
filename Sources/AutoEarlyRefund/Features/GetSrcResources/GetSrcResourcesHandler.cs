using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesInfo;

public interface IGetSrcResourcesHandler
{
    Task<List<SourcesConditions>> HandleAsync(List<int> matchIds, List<SourcesConditions> sourcesConditions);
}

public class GetSrcResourcesHandler : IGetSrcResourcesHandler
{
    private readonly IDynamicRepository repository;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GetSrcResourcesHandler(IDynamicRepository repository)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<SourcesConditions>> HandleAsync(List<int> matchIds, List<SourcesConditions> sourcesConditions)
    {
        var groupedSources = GetDistinctGroupedSources(sourcesConditions, matchIds);
        var resources = await FetchResources(groupedSources);

        return MapSourcesWithResources(sourcesConditions, resources);
    }

    private async Task<List<Resource>> FetchResources(List<ResourceInfoItemRequest> groupedSources)
    {
        var json = SerializeToJson(groupedSources);
        var criteria = new GetSrcResourcesCriteria(json);
        var resources = await repository.FetchAsync<Resource>(criteria);

        return resources.ToList();
    }

    private static List<ResourceInfoItemRequest> GetDistinctGroupedSources(
        List<SourcesConditions> sourcesConditions,
        List<int> matchIds)
    {

        return [.. matchIds.SelectMany(matchId =>
            sourcesConditions
                .Select(sc => new ResourceInfoItemRequest
                {
                    SportType = sc.SportType,
                    SourceMatchType = sc.SourceMatchType,
                    SourceChildMatchType = sc.SourceChildMatchType,
                    SourceResource = sc.SourceResource,
                    MatchId = matchId
                }))
             .DistinctBy(x => new
             {
                 x.SourceMatchType,
                 x.SourceChildMatchType,
                 x.SourceResource,
                 x.SportType,
                 x.MatchId
             })];
    }

    private static List<SourcesConditions> MapSourcesWithResources(
        List<SourcesConditions> sourcesConditions,
        List<Resource> resources)
    {
        return [.. sourcesConditions
            .GroupJoin(
                resources,
                sc => CreateJoinKey(sc),
                s => CreateResourceKey(s),
                (sc, s) => CreateMappedCondition(sc, s.FirstOrDefault()))];
    }

    private static object CreateJoinKey(SourcesConditions sc) => new
    {
        sc.SourceResourceId,
        sc.SourceMatchType,
        sc.SourceChildMatchType
    };

    private static object CreateResourceKey(Resource s) => new
    {
        SourceResourceId = s.SourceResource,
        s.SourceMatchType,
        s.SourceChildMatchType
    };

    private static SourcesConditions CreateMappedCondition(SourcesConditions sc, Resource? resource)
    {
        return new SourcesConditions
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
            Resource = resource
        };
    }

    private static string SerializeToJson<T>(T value) =>
        JsonSerializer.Serialize(value, SerializerOptions);
}

