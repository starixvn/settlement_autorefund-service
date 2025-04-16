using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesConditions;

public interface IGetResourcesConditionsHandler
{
    Task<List<SourcesConditions>> GetConditionsAsync(List<SourcesConditionsItem> sourcesConditionsItems);
    Task<List<SourcesConditions>> GetConditionsAsync(short matchType, List<SourcesConditionsItem> targetResourceTriggers);
}

public class GetResourcesConditionsHandler(IDynamicRepository repository) : IGetResourcesConditionsHandler
{
    public async Task<List<SourcesConditions>> GetConditionsAsync(short matchType, List<SourcesConditionsItem> targetResourceTriggers)
    {
        var request = targetResourceTriggers
            .Select(x => x.WithMatchType(matchType))
            .ToList();

        var conditions = await GetConditionsAsync(request);

        return conditions;
    }

    public async Task<List<SourcesConditions>> GetConditionsAsync(List<SourcesConditionsItem> sourcesConditionsItems)
    {
        var json = JsonSerializer.Serialize(
            sourcesConditionsItems,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

        var results = await repository.FetchAsync<SourcesConditions>(new GetResourcesConditionsCriteria(json));

        return [.. results.SelectMany(x => x.SourceResourceIds.Select(sr => new SourcesConditions
        {
            Id = x.Id,
            SportType = x.SportType,
            ResourceId = x.ResourceId,
            RuleId = x.RuleId,
            RuleName = x.RuleName,
            SourceAction = x.SourceAction,
            MatchType = x.MatchType,
            ChildMatchType = x.ChildMatchType,
            SourceMatchType = x.SourceMatchType,
            SourceChildMatchType = x.SourceChildMatchType,
            SourceResource = x.SourceResource,
            SourceResourceId = sr,
            Conditions = x.Conditions,
        }))];
    }
}