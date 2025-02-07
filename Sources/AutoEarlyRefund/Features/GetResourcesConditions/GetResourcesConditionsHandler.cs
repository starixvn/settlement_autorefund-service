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

        public async Task<List<SourcesConditions>> HandleAsync(List<TargetResourceTrigger> targetResourceTriggers)
        {
            var x = targetResourceTriggers
                .SelectMany(x => x.TargetResourceIds).Distinct()
                .Where(x => x == "38301" || x == "38401")
                .ToList();
            var z = x.Select(x => new
            {
                ResourceId = x,
                SportType = 1,
                childMatchType = 202,
                MatchType = 2
            }).ToList();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(z, options);

            var results = await repository.FetchAsync<SourcesConditions>(new GetResourcesConditionsCriteria
            {
                ResourceInfo = json
            });



            return results.ToList();
        }
    }
}