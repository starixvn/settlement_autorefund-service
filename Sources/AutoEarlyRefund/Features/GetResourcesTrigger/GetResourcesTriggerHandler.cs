using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.GetResourcesTrigger;

public class GetResourcesTriggerHandler
{
    private readonly IDynamicRepository repository;

    public GetResourcesTriggerHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<TargetResourceTrigger>> GetTargetResourceTriggersAsync(
        GetResourcesTriggerCriteria criteria)
    {
        var results = await repository.FetchAsync<TargetResourceTrigger>(criteria);

        return results.ToList();
    }
}

