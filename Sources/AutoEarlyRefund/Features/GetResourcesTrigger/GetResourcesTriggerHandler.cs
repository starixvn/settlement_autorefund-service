using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesTrigger;

public class GetResourcesTriggerHandler(IDynamicRepository repository)
{
    public async Task<List<TargetResourceTrigger>> GetTargetResourceTriggersAsync(
        GetResourcesTriggerCriteria criteria)
    {
        var results = await repository.FetchAsync<TargetResourceTrigger>(criteria);

        return results.ToList();
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
        var results = await repository.FetchAsync<TargetResourceTrigger>(criteria);

        return results.ToList();
    }
}

