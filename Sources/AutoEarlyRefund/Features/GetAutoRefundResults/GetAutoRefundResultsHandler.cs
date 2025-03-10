using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetAutoRefundResults;

public class GetAutoRefundResultsHandler
{
    private readonly IDynamicRepository repository;

    public GetAutoRefundResultsHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<SourcesConditionsItem>> Handle(int matchId, List<SourcesConditionsItem> sourcesConditions)
    {
        var request = sourcesConditions.Select(x => new GetAutoRefundResultsItem(matchId, x.ResourceId)).ToList();

        var json = SerializeToJson(request);

        var criteria = new GetAutoRefundResultsCriteria
        {
            ResourceInfo = json
        };

        var autoRefundResults = await repository.FetchAsync<ResultDetail>(criteria);
        var autoRefundResultIds = autoRefundResults.Select(x => x.ResourceId);

        return sourcesConditions
            .Where(x => !autoRefundResultIds.Contains(x.ResourceId))
            .ToList();

    }

    private static string SerializeToJson(List<GetAutoRefundResultsItem> request)
        => JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

}
