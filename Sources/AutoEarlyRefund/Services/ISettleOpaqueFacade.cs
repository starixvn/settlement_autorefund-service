using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Services;

public interface ISettleOpaqueFacade
{
    Task Settle(Match match);
}


public class SettleOpaqueFacadeImp(
    ScaleOutSettleService scaleOutSettleService,
    IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
    IPushResultSettlementHandler pushResultSettlementHandler,
    IWriteLogService writeLogService,
    IValidateHandler validateHandler) : ISettleOpaqueFacade
{

    public async Task Settle(Match match)
    {
        await validateHandler.HandleAsync(match);

        await writeLogService.WriteLogPreSettlement(match);
        await writeLogService.WriteLogSettle(match);

        await insOrUpdAutoPayoutResultHandler.Handle(match, 1);
        await UpdateMatch(match);

        await pushResultSettlementHandler.HandleAsync(match, CancellationToken.None);

        await insOrUpdAutoPayoutResultHandler.Handle(match, 2);
    }


    private async Task UpdateMatch(Match match)
    {
        var updateMatchInfoRequest = new UpdateMatchInfoRequest
        {
            Action = "REFUND",
            IsAuto = true,
            MatchId = match.MatchId,
            SelectedResources = match.ResourcesXml,
        };

        await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);
    }
}
