using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

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
    ILogger<SettleOpaqueFacadeImp> logger,
    XmlConvertService xmlConvertService) : ISettleOpaqueFacade
{

    public async Task Settle(Match match)
    {
        Fanex.Logging.LogService.Info($"Process Settle {match.MatchId} {match}");
        logger.LogInformation($"Process Settle {match.MatchId} {match}");
        try
        {
            var result = await validateHandler.HandleAsync(match);
            if (!result.IsSuccess)
            {
                Fanex.Logging.LogService.Warn($"SettleOpaqueFacade: {match} {result}");
                logger.LogWarning($"SettleOpaqueFacade: {match} {result}");
                await HandleErrorValidMatch(match, result.ErrMsg);
                return;
            }
            var newMatch = match.WithResourcesXml(xmlConvertService);
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
                newMatch.ResultDetails.Select(x => new ResourceItem
                {
                    ResourceId = x.ResourceId,
                    HomeValue = x.HomeValue,
                    AwayValue = x.AwayValue,
                }).ToList()), new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);
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
