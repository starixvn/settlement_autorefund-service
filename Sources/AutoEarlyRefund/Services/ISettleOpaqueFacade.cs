using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using Fanex.Settlement.MessageBridge.RefundEvents;
using MassTransit;
using ResourceItem = Fanex.Settlement.MessageBridge.RefundEvents.ResourceItem;

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
    IPublishEndpoint publishEndpoint,
    XmlConvertService xmlConvertService) : ISettleOpaqueFacade
{

    public async Task Settle(Match match)
    {
        Fanex.Logging.LogService.Info($"Process Settle {match.MatchId} {match.ResultDetails.Count}");
        try
        {
            var result = await validateHandler.HandleAsync(match);
            if (!result.IsSuccess)
            {
                await HandleErrorValidMatch(match, result.ErrMsg);
                return;
            }
            var newMatch = match.WithResourcesXml(xmlConvertService);

            Fanex.Logging.LogService.Info($"newMatch Process Settle {match.MatchId} {match.ResultDetails.Count}");


            await writeLogService.WriteLogPreSettlement(newMatch);
            await writeLogService.WriteLogSettle(newMatch);

            await insOrUpdAutoPayoutResultHandler.Handle(newMatch, AutoRefund.Core.ResultRefundStatus.Running);

            await UpdateMatch(newMatch);

            await pushResultSettlementHandler.HandleAsync(newMatch, CancellationToken.None);

            await insOrUpdAutoPayoutResultHandler.Handle(newMatch, AutoRefund.Core.ResultRefundStatus.Completed);

            await publishEndpoint.Publish(new ResultsRefunded(
                newMatch.MatchId,
                newMatch.SportType,
                newMatch.ChildMatchType,
                [.. newMatch.ResultDetails.Select(x => new ResourceItem
                {
                    ResourceId = x.ResourceId,
                    HomeValue = x.HomeValue,
                    AwayValue = x.AwayValue,
                })]), new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);
        }
        catch (AppException ex)
        {
            Fanex.Logging.LogService.Warn(ex.Message, ex);

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

    private async Task HandleErrorValidMatch(Match match, string errorMsg)
    {
        match.ResultDetails.ForEach(x => x.Remark = errorMsg);

        var newMatch = match.WithResourcesXml(xmlConvertService, false);

        await insOrUpdAutoPayoutResultHandler.Handle(newMatch, AutoRefund.Core.ResultRefundStatus.Failed);
        await writeLogService.WriteLogErrorSettle(newMatch, errorMsg + ": " + string.Join(",", newMatch.ResultDetails.Select(x => x.ResourceId)));
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
