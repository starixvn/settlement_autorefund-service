using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;

public interface IInsOrUpdAutoPayoutResultHandler
{
    Task Handle(Match match, int statusId);
}

public class InsOrUpdAutoPayoutResultHandler : IInsOrUpdAutoPayoutResultHandler
{
    private readonly IDynamicRepository repository;

    public InsOrUpdAutoPayoutResultHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }

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
