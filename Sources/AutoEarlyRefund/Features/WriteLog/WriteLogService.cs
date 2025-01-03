using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.WriteLog;

public interface IWriteLogService
{
    Task WriteLogErrorSettle(Match match, string errorMessage);
    Task WriteLogPreSettlement(Match match);
    Task WriteLogSettle(Match match);
}

public class WriteLogService : IWriteLogService
{
    private readonly IDynamicRepository repository;
    public WriteLogService(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task WriteLogPreSettlement(Match match)
    {
        await repository.ExecuteAsync(new WriteLogPreSettlementCriteria
        {
            Doing = $"NAP: EAR REFUND: {string.Join(", ", match.ResultDetails.Select(x => x.ToString()))}",
            Extend = match.MatchId,
        });
    }

    public async Task WriteLogSettle(Match match)
    {
        var criteria = new WriteLogSettleCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            IsAuto = true,
            SelectedResources = match.ResourcesXml
        };

        await repository.ExecuteAsync(criteria);
    }


    public async Task WriteLogErrorSettle(Match match, string errorMessage)
    {
        await repository.ExecuteAsync(new WriteLogErrorSettleCriteria
        {
            MatchId = match.MatchId,
            Remark = errorMessage,
            //ActionId = ActionId.EarlyType
        });
    }


}
