using AutoRefund.Constants;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using Microsoft.Extensions.Options;

namespace AutoRefund.Services;

public interface IMatchRefundController
{
    Task Process(Match match);
}

public class MatchRefundControllerImp : IMatchRefundController
{
    private readonly IMatchPoolService matchPoolService;

    private readonly IValidationService validationService;

    private readonly IRefundMatchService refundMatchService;

    private readonly IMatchService matchService;

    private readonly AppSettingsKey appSettingsKey;

    private readonly LogService logService;

    public MatchRefundControllerImp(
        IMatchPoolService matchPoolService,
        IValidationService validationService,
        IRefundMatchService refundMatchService,
        IMatchService matchService,
        LogService logService,
        IOptions<AppSettingsKey> appSettingsKey)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.matchPoolService = matchPoolService;
        this.validationService = validationService;
        this.refundMatchService = refundMatchService;
        this.matchService = matchService;
        this.logService = logService;
    }

    public async Task Process(Match match)
    {
        var started = true;
        match.IsChecking = true;
        var retryCount = 0;
        await matchService.UpdateStatus(match, MatchStatus.Running);
        while (started && retryCount < appSettingsKey.RetryCount + 1)
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
            await matchService.UpdateRemark(match, result.ErrMsg, MatchStatus.Failed);
        }
        matchPoolService.RemoveMatch(match);
    }

    private async Task ProcessStopLoop(Match match, string error)
    {
        await logService.ActionLog(match, error);
        await matchService.UpdateRemark(match, error, MatchStatus.Failed);
        matchPoolService.RemoveMatch(match);
    }

    private async Task<int> ProcessWhenCachedException(int retryCount, Match match, AutoRefundException exception)
    {
        await matchService.UpdateRemark(match, exception.Description, MatchStatus.Running);
        if (retryCount >= appSettingsKey.RetryCount)
        {
            await logService.ActionLog(match, exception.Description);
            matchPoolService.RemoveAndUpdateStatus(match, MatchStatus.Failed);
        }
        else
        {
            await Task.Delay(appSettingsKey.RetryInterval);
        }

        return retryCount + 1;
    }
}