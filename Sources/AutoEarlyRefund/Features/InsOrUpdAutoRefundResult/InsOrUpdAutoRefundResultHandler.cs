using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;

public interface IInsOrUpdAutoPayoutResultHandler
{
    Task Handle(Match match, int statusId);
}

public class InsOrUpdAutoRefundResultHandler : IInsOrUpdAutoPayoutResultHandler
{
    private readonly IDynamicRepository repository;

    public InsOrUpdAutoRefundResultHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task Handle(Match match, int statusId)
    {
        var criteria = new InsOrUpdAutoRefundResultCriteria
        {
            MatchId = match.MatchId,
            StatusId = statusId,
            SelectedResources = match.ResourcesXml
        };
        await repository.ExecuteAsync(criteria);
    }
}
