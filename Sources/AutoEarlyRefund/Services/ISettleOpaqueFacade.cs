using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using AutoRefund.Core;

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
    IValidateHandler validateHandler,
    MatchPoolService matchPoolService) : ISettleOpaqueFacade
{

    public async Task Settle(Match match)
    {
        try
        {
            var result = await validateHandler.HandleAsync(match);
            if (!result.IsSuccess)
            {
                return;
            }
            await writeLogService.WriteLogPreSettlement(match);
            await writeLogService.WriteLogSettle(match);

            await insOrUpdAutoPayoutResultHandler.Handle(match, MatchStatus.Running);

            await UpdateMatch(match);

            await pushResultSettlementHandler.HandleAsync(match, CancellationToken.None);

            await insOrUpdAutoPayoutResultHandler.Handle(match, MatchStatus.Completed);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            matchPoolService.Remove(match);
        }
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
