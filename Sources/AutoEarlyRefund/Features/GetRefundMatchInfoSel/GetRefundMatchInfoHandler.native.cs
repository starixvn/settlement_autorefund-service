using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoEarlyRefund.Features.GetRefundMatchInfoSel;

public interface IGetRefundMatchInfoHandler
{
    Task<List<RelatedMatchItem>> HandleAsync(List<GetRelatedMatchItemRequest> request);
}

public class GetRefundMatchInfoHandler(IDynamicRepository repository) : IGetRefundMatchInfoHandler
{
    public async Task<List<RelatedMatchItem>> HandleAsync(List<GetRelatedMatchItemRequest> request)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(request, options);

        var criteria = new GetRefundMatchInfoCriteria
        {
            MatchInfo = json
        };

        var result = await repository.FetchAsync<RelatedMatchItem>(criteria);

        return result.ToList();
    }
}