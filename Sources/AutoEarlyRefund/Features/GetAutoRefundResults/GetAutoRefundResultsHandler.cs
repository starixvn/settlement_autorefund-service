using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;
using AutoRefund.Core.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                .Select(sourceCondition => new GetRefundResultsItem(
                    matchId,
                    sourceCondition.ResourceId,
                    7, // Assuming TypeId is 7 for Auto Early Refund
                    string.Empty)))
            .ToList();

        var criteria = new GetAutoRefundResultsCriteria
        {
            ResourceInfo = SerializeToJson(requestItems)
        };

        var autoRefundResults = await repository.FetchAsync<ResultDetail>(criteria);

        var autoRefundResultIds = new HashSet<string>(autoRefundResults.Select(x => x.ResourceId));

        return [.. sourcesConditions.Where(x => !autoRefundResultIds.Contains(x.ResourceId))];
    }

    private static string SerializeToJson(List<GetRefundResultsItem> request)
        => JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
}