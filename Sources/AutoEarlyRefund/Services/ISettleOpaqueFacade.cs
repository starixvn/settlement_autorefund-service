using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using MassTransit;

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
    MatchPoolService matchPoolService,
    IPublishEndpoint publishEndpoint) : ISettleOpaqueFacade
{

    public async Task Settle(Match match)
    {
        try
        {
            var result = await validateHandler.HandleAsync(match);
            if (!result.IsSuccess)
            {
                match.ResultDetails.ForEach(x => x.Remark = result.ErrMsg);
                await insOrUpdAutoPayoutResultHandler.Handle(match, AutoRefund.Core.ResultRefundStatus.Failed);
                return;
            }
            await writeLogService.WriteLogPreSettlement(match);
            await writeLogService.WriteLogSettle(match);

            await insOrUpdAutoPayoutResultHandler.Handle(match, AutoRefund.Core.ResultRefundStatus.Running);

            await UpdateMatch(match);

            await pushResultSettlementHandler.HandleAsync(match, CancellationToken.None);

            await insOrUpdAutoPayoutResultHandler.Handle(match, AutoRefund.Core.ResultRefundStatus.Completed);

            await publishEndpoint.Publish(new ResultsRefunded(
                match.MatchId,
                match.SportType,
                match.ChildMatchType,
                match.ResultDetails.Select(x => new ResourceItem
                {
                    ResourceId = x.ResourceId,
                    HomeValue = x.HomeValue,
                    AwayValue = x.AwayValue,
                }).ToList()), new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);
        }
        catch (Exception ex)
        {
            Fanex.Logging.LogService.Error(ex.Message, ex);
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
