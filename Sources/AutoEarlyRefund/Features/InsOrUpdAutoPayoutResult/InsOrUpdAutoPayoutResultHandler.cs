using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;

public interface IInsOrUpdAutoPayoutResultHandler
{
    Task Handle(Match match, int statusId);
}

public class InsOrUpdAutoPayoutResultHandler(IDynamicRepository repository) : IInsOrUpdAutoPayoutResultHandler
{
    public async Task Handle(Match match, int statusId)
    {
        await repository.ExecuteAsync(new AutoPayoutResultInsOrUpdCriteria
        {
            MatchId = match.MatchId,
            StatusId = statusId,
            SelectedResources = match.ResourcesXml
        });
    }
}