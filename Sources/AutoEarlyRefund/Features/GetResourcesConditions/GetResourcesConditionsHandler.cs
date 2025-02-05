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

        public async Task<List<SourcesConditions>> HandleAsync(
            short sportType,
            short matchType,
            short childMatchType,
            List<TargetResourceTrigger> targetResourceTriggers)
        {
            var targetResources = targetResourceTriggers.First().TargetResource.Split(',').ToList();
            var x = targetResources
                .Where(x => x == "38301")
                .Select(x => new
                {
                    ResourceId = x,
                    SportType = sportType,
                    childMatchType = childMatchType,
                    matchType = matchType
                }).ToList();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(x, options);

            var results = await repository.FetchAsync<SourcesConditions>(new GetResourcesConditionsCriteria
            {
                ResourceInfo = json
            });



            return results.ToList();
        }
    }
}