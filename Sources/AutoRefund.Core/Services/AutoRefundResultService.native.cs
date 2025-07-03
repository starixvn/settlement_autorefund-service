using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoRefund.Core.Services;


public record GetRefundResultsItem(int MatchId, string ResourceId, int TypeId, string OtherInfo);

public interface AutoRefundResultService
{
    Task<List<AutoRefundResult>> GetResults(List<GetRefundResultsItem> refundResultsItems);
    Task InsertOrUpdateResults(int matchId, int statusId, string resourceXml);
}

public class AutoRefundResultServiceImp(
    IDynamicRepository repository) : AutoRefundResultService
{
    public async Task<List<AutoRefundResult>> GetResults(List<GetRefundResultsItem> refundResultsItems)
    {
        var criteria = new GetAutoRefundResultsCriteria
        {
            ResourceInfo = SerializeToJson(refundResultsItems)
        };

        var results = await repository.FetchAsync<AutoRefundResult>(criteria);

        return [.. results];
    }

    public async Task InsertOrUpdateResults(
        int matchId,
        int statusId,
        string resourceXml)
    {
        await repository.ExecuteAsync(new InsOrUpdAutoRefundResultCriteria
        {
            MatchId = matchId,
            StatusId = statusId,
            SelectedResources = resourceXml
        });
    }

    private static string SerializeToJson(List<GetRefundResultsItem> request)
    => JsonSerializer.Serialize(request, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });
}
