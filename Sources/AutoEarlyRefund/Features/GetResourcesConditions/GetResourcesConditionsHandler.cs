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
                    x.ChildMatchType,
                    x.TargetResource
                })
                .Select(x => new SourcesConditionsItemRequest
                {
                    ResourceId = x.TargetResourceIds.First(),
                    SportType = x.SportType,
                    childMatchType = x.ChildMatchType,
                    MatchType = x.MatchType,
                    RuleId = 0,
                })
            .ToList();

            return await GetConditionsAsync(x);
        }

        public async Task<List<SourcesConditions>> GetConditionsAsync(List<SourcesConditionsItemRequest> request)
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