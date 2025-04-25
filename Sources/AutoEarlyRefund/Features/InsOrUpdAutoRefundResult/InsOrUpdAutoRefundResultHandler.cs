using AutoEarlyRefund.Models;
using Fanex.Logging;

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
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task Handle(Match match, int statusId)
    {
        Logger.Log.Info($"InsOrUpdAutoRefundResultHandler.Handle: {match}");

        var criteria = new InsOrUpdAutoRefundResultCriteria
        {
            MatchId = match.MatchId,
            StatusId = statusId,
            SelectedResources = match.ResourcesXml
        };

        await repository.ExecuteAsync(criteria);
    }
}
