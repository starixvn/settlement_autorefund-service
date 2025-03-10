using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetRefundResults;

public class GetResultDetailsHandler(IDynamicRepository repository)
{
    public async Task<List<SourcesConditionsItem>> Handle(int matchId, List<TargetResourceTrigger> targetResourceTriggers)
    {
        var sourcesConditions = GetSourcesConditionsItems(targetResourceTriggers);

        var results = await GetResults(matchId, sourcesConditions);

        var validSourcesConditions = sourcesConditions.Where(x => !results.Exists(r => r.ResourceId == x.ResourceId)).ToList();

        return validSourcesConditions;
    }

    private async Task<List<ResultDetail>> GetResults(int matchId, List<SourcesConditionsItem> sourcesConditions)
    {
        var request = sourcesConditions
            .Select(x => new GetResultDetailsItem(matchId, x.SportType, x.ResourceId))
            .ToList();

        var json = SerializeToJson(request);

        var criteria = new GetResultDetailsCriteria
        {
            ResourceInfo = json
        };

        var results = await repository.FetchAsync<ResultDetail>(criteria);

        return results
            .Where(x => x.ResultStatusId == (int)ResultStatus.ManualSettled || x.ResultStatusId == (int)ResultStatus.AutoSettled)
            .ToList();
    }

    private static string SerializeToJson(List<GetResultDetailsItem> groupedSources)
        => JsonSerializer.Serialize(groupedSources, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

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
                ResourceId = x.TargetResourceIds[0],
                SportType = x.SportType,
                ChildMatchType = (short)x.TargetResourceChildMatchType,
                MatchType = x.MatchType,
            })
            .ToList();
    }
}
