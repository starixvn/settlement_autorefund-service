using Fanex.Data.Repository;

namespace AutoRefundFM.Features.InsOrUpdAutoRefundResult;

public interface IInsOrUpdAutoPayoutResultHandler
{
    Task Handle(int matchId, int statusId, string resourceXml);

}

public class InsOrUpdAutoRefundResultHandler(IDynamicRepository repository) : IInsOrUpdAutoPayoutResultHandler
{
    public async Task Handle(int matchId, int statusId, string resourceXml)
    {

        var criteria = new InsOrUpdAutoRefundResultCriteria
        {
            MatchId = matchId,
            StatusId = statusId,
            SelectedResources = resourceXml,
        };

        await repository.ExecuteAsync(criteria);
    }

}