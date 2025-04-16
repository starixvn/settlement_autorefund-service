using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesTrigger;

public interface IGetResourcesTriggerHandler
{
    Task<List<TargetResourceTrigger>> GetTargetResourceTriggersAsync(GetResourcesTriggerCriteria criteria);
    Task<List<TargetResourceTrigger>> GetTargetResourceTriggersAsync(string action, short shortType, short childMatchType, string resourceIds);
}

public class GetResourcesTriggerHandler(IDynamicRepository repository) : IGetResourcesTriggerHandler
{
    public async Task<List<TargetResourceTrigger>> GetTargetResourceTriggersAsync(
        GetResourcesTriggerCriteria criteria)
    {
        var results = await repository.FetchAsync<TargetResourceTrigger>(criteria);

        return [.. results];
    }

    public async Task<List<TargetResourceTrigger>> GetTargetResourceTriggersAsync(
        string action,
        short shortType,
        short childMatchType,
        string resourceIds)

    {
        var criteria = new GetResourcesTriggerCriteria
        {
            Action = action,
            SportType = shortType,
            ChildMatchType = childMatchType,
            ResourceIds = resourceIds
        };
        var results = await GetTargetResourceTriggersAsync(criteria);

        return results.ToList();
    }
}

