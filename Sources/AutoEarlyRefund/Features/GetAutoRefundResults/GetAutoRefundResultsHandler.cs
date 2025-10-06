using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetAutoRefundResults;

public interface IGetAutoRefundResultsHandler
{
    Task<List<SourcesConditionsItem>> Handle(List<int> matchIds, List<SourcesConditionsItem> sourcesConditions);
}

public class GetAutoRefundResultsHandler(IDynamicRepository repository) : IGetAutoRefundResultsHandler
{
    public async Task<List<SourcesConditionsItem>> Handle(List<int> matchIds, List<SourcesConditionsItem> sourcesConditions)
    {
        var requestItems = matchIds.SelectMany(matchId => sourcesConditions
                .Select(sourceCondition => new GetAutoRefundResultsItem(matchId, sourceCondition.ResourceId)))
            .ToList();

        var criteria = new GetAutoRefundResultsCriteria
        {
            ResourceInfo = SerializeToJson(requestItems)
        };

        var autoRefundResults = await repository.FetchAsync<ResultDetail>(criteria);

        var autoRefundResultIds = new HashSet<string>(autoRefundResults.Select(x => x.ResourceId));

        return [.. sourcesConditions.Where(x => !autoRefundResultIds.Contains(x.ResourceId))];
    }

    private static string SerializeToJson(List<GetAutoRefundResultsItem> request)
        => JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
}