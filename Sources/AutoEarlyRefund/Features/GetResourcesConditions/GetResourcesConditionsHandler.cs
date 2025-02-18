using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesConditions
{
    public class GetResourcesConditionsHandler
    {
        private readonly IDynamicRepository repository;

        public GetResourcesConditionsHandler(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<List<SourcesConditions>> GetConditionsAsync(List<TargetResourceTrigger> targetResourceTriggers)
        {
            var x = targetResourceTriggers
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
                    RuleId = 0,
                })

            .ToList();

            return await GetConditionsAsync(x);
        }

        public async Task<List<SourcesConditions>> GetConditionsAsync(List<SourcesConditionsItem> request)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(request, options);

            var results = await repository.FetchAsync<SourcesConditions>(new GetResourcesConditionsCriteria
            {
                ResourceInfo = json
            });

            return results.ToList();
        }
    }
}