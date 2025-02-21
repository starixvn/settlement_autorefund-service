namespace AutoRefund.Services;

using AutoRefund.Core.Models;
using Core;
using Exceptions;
using Models;

public interface IMatchRefundController
{
    Task Process(Match match);
}

public class MatchRefundControllerImp(
    IMatchPoolService matchPoolService,
    IValidationService validationService,
    IRefundMatchService refundMatchService,
    IMatchService matchService,
    LogService logService,
    IOptions<AppSettingsKey> appSettingsKey)
    : IMatchRefundController
{
    public async Task Process(Match match)
    {
        var started = true;
        match.IsChecking = true;
        var retryCount = 0;
        await matchService.UpdateStatus(match, ResultRefundStatus.Running);
        while (started && retryCount < appSettingsKey.Value.RetryCount + 1)
        {
            try
            {
                await validationService.VerifyMatchBeforeExecution(match);
                var result = await refundMatchService.Push(match);
                await ProcessAfterRefund(match, result);
                started = false;
            }
            catch (AutoRefundException exception)
            {
                if (!exception.StopLoop)
                {
                    retryCount = await ProcessWhenCachedException(retryCount, match, exception);
                }
                else
                {
                    await ProcessStopLoop(match, exception.Description);
                    started = false;
                }
            }
        }
    }

    private async Task ProcessAfterRefund(Match match, Result result)
    {
        if (!string.IsNullOrWhiteSpace(result.ErrMsg))
        {
            await logService.ActionLog(match, result.ErrMsg);
            await matchService.UpdateRemark(match, result.ErrMsg, ResultRefundStatus.Failed);
        }
        matchPoolService.RemoveMatch(match);
    }

    private async Task ProcessStopLoop(Match match, string error)
    {
        await logService.ActionLog(match, error);
        await matchService.UpdateRemark(match, error, ResultRefundStatus.Failed);
        matchPoolService.RemoveMatch(match);
    }

    private async Task<int> ProcessWhenCachedException(int retryCount, Match match, AutoRefundException exception)
    {
        await matchService.UpdateRemark(match, exception.Description, ResultRefundStatus.Running);
        if (retryCount >= appSettingsKey.Value.RetryCount)
        {
            await logService.ActionLog(match, exception.Description);
            matchPoolService.RemoveAndUpdateStatus(match, ResultRefundStatus.Failed);
        }
        else
        {
            await Task.Delay(appSettingsKey.Value.RetryInterval);
        }

        return retryCount + 1;
    }
}