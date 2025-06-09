using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoEarlyRefund.Features.GetResultDetails;

public interface IGetResultDetailsHandler
{
    Task<List<SourcesConditionsItem>> Handle(List<int> matchIds, List<TargetResourceTrigger> targetResourceTriggers);
}

public class GetResultDetailsHandler(IDynamicRepository repository) : IGetResultDetailsHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<List<SourcesConditionsItem>> Handle(List<int> matchIds, List<TargetResourceTrigger> targetResourceTriggers)
    {
        var sourcesConditions = GetSourcesConditionsItems(targetResourceTriggers);
        var results = await GetResults(matchIds, sourcesConditions);

        return sourcesConditions
            .Where(x => !results.Exists(r => r.ResourceId == x.ResourceId))
            .ToList();
    }

    private async Task<List<ResultDetail>> GetResults(List<int> matchIds, List<SourcesConditionsItem> sourcesConditions)
    {
        var request = matchIds
            .SelectMany(matchId => sourcesConditions
            .Select(sourceCondition => new GetResultDetailsItem(matchId, sourceCondition.SportType, sourceCondition.ResourceId)))
            .ToList();

        var criteria = new GetResultDetailsCriteria
        {
            ResourceInfo = SerializeToJson(request)
        };

        var results = await repository.FetchAsync<ResultDetail>(criteria);

        return [.. results.Where(x => IsSettled(x.ResultStatusId))];
    }

    private static bool IsSettled(int resultStatusId) =>
        resultStatusId is (int)ResultStatus.ManualSettled or (int)ResultStatus.AutoSettled;

    private static string SerializeToJson<T>(T value) =>
        JsonSerializer.Serialize(value, SerializerOptions);

    private static List<SourcesConditionsItem> GetSourcesConditionsItems(List<TargetResourceTrigger> triggers)
    {
        return [.. triggers
            .SelectMany(trigger => trigger.TargetResourceIds.Select(resourceId =>
                MapToTargetResourceTrigger(trigger, resourceId)))
            .DistinctBy(x => new
            {
                x.SportType,
                x.MatchType,
                x.TargetResourceChildMatchType,
                x.TargetResource
            })
            .Select(MapToSourcesConditionsItem)
        ];
    }

    private static TargetResourceTrigger MapToTargetResourceTrigger(TargetResourceTrigger source, string resourceId)
    {
        return new TargetResourceTrigger
        {
            Id = source.Id,
            SportType = source.SportType,
            Action = source.Action,
            MatchType = source.MatchType,
            ChildMatchType = source.ChildMatchType,
            Conditions = source.Conditions,
            TargetIsCrrMatch = source.TargetIsCrrMatch,
            TargetIsCrrResource = source.TargetIsCrrResource,
            TargetResourceChildMatchType = source.TargetResourceChildMatchType,
            TargetResource = resourceId
        };
    }

    private static SourcesConditionsItem MapToSourcesConditionsItem(TargetResourceTrigger trigger)
    {
        return new SourcesConditionsItem
        {
            ResourceId = trigger.TargetResourceIds[0],
            SportType = trigger.SportType,
            ChildMatchType = (short)trigger.TargetResourceChildMatchType,
            MatchType = trigger.MatchType
        };
    }
}