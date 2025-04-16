using System.Text.Json;
using System.Text.Json.Serialization;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetRefundResults;

public interface IGetResultDetailsHandler
{
    Task<List<SourcesConditionsItem>> Handle(int matchId, List<TargetResourceTrigger> targetResourceTriggers);
}

public class GetResultDetailsHandler : IGetResultDetailsHandler
{
    private readonly IDynamicRepository repository;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GetResultDetailsHandler(IDynamicRepository repository)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<SourcesConditionsItem>> Handle(int matchId, List<TargetResourceTrigger> targetResourceTriggers)
    {
        var sourcesConditions = GetSourcesConditionsItems(targetResourceTriggers);
        var results = await GetResults(matchId, sourcesConditions);

        return sourcesConditions
            .Where(x => !results.Exists(r => r.ResourceId == x.ResourceId))
            .ToList();
    }

    private async Task<List<ResultDetail>> GetResults(int matchId, List<SourcesConditionsItem> sourcesConditions)
    {
        var request = sourcesConditions
            .Select(x => new GetResultDetailsItem(matchId, x.SportType, x.ResourceId))
            .ToList();

        var criteria = new GetResultDetailsCriteria
        {
            ResourceInfo = SerializeToJson(request)
        };

        var results = await repository.FetchAsync<ResultDetail>(criteria);

        return [.. results.Where(x => IsSettled(x.ResultStatusId))];
    }

    private static bool IsSettled(int resultStatusId) =>
        resultStatusId == (int)ResultStatus.ManualSettled ||
        resultStatusId == (int)ResultStatus.AutoSettled;

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
            .Select(MapToSourcesConditionsItem)];
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
