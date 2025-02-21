using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesConditions
{
    public class GetResourcesConditionsHandler(IDynamicRepository repository)
    {
        public async Task<List<SourcesConditions>> GetConditionsAsync(short matchType, List<TargetResourceTrigger> targetResourceTriggers)
        {
            var sourcesConditionsItems = GetSourcesConditionsItems(targetResourceTriggers)
                .Select(x => x.WithMatchType(matchType))
                .ToList();

            var conditions = await GetConditionsAsync(sourcesConditionsItems);

            return conditions.SelectMany(x => x.SourceResourceIds.Select(sr => new SourcesConditions
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
            })).ToList();
        }

        public async Task<List<SourcesConditions>> GetConditionsAsync(List<SourcesConditionsItem> request)
        {
            var json = JsonSerializer.Serialize(
                request,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

            var results = await repository.FetchAsync<SourcesConditions>(new GetResourcesConditionsCriteria(json));

            return results.ToList();
        }


        private List<SourcesConditionsItem> GetSourcesConditionsItems(List<TargetResourceTrigger> targetResourceTriggers)
        {
            return targetResourceTriggers
                .SelectMany(x => x.TargetResourceIds.Select(r => new TargetResourceTrigger
                {
                    Id = x.Id,
                    SportType = x.SportType,
                    Action = x.Action,
                    MatchType = x.MatchType,
                    ChildMatchType = x.ChildMatchType,
                    Conditions = x.Conditions,
                    TargetIsCrrMatch = x.TargetIsCrrMatch,
                    TargetIsCrrResource = x.TargetIsCrrResource,
                    TargetResourceChildMatchType = x.TargetResourceChildMatchType,
                    TargetResource = r

                })).DistinctBy(x => new
                {
                    x.SportType,
                    x.MatchType,
                    x.TargetResourceChildMatchType,
                    x.TargetResource
                })
                .Select(x => new SourcesConditionsItem
                {
                    ResourceId = x.TargetResourceIds.First(),
                    SportType = x.SportType,
                    ChildMatchType = (short)x.TargetResourceChildMatchType,
                    MatchType = x.MatchType,
                })
                .ToList();
        }
    }
}