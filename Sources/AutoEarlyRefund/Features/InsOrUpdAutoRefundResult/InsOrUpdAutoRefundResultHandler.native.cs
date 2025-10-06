using AutoEarlyRefund.Models;
using Fanex.Logging;

namespace AutoEarlyRefund.Features.InsOrUpdAutoRefundResult;

public interface IInsOrUpdAutoPayoutResultHandler
{
    Task Handle(Match match, int statusId);

    Task Handle(List<Match> matches, int statusId);
}

public class InsOrUpdAutoRefundResultHandler(IDynamicRepository repository) : IInsOrUpdAutoPayoutResultHandler
{
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

    public async Task Handle(List<Match> matches, int statusId)
    {
        foreach (var item in matches)
        {
            await Handle(item, statusId);
        }
    }
}