using AutoRefund.Constants;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using Microsoft.Extensions.Options;
using System.Threading;

namespace AutoRefund.Services
{
    public interface IMatchRefundController
    {
        void Process(Match match);
    }

    public class MatchRefundControllerImp : IMatchRefundController
    {
        private readonly IMatchPoolService matchPoolService;

        private readonly IValidationService validationService;

        private readonly IRefundMatchService refundMatchService;

        private readonly IMatchService matchService;

        private readonly AppSettingsKey appSettingskey;

        private readonly ILogService logService;

        public MatchRefundControllerImp(
            IMatchPoolService matchPoolService,
            IValidationService validationService,
            IRefundMatchService refundMatchService,
            IMatchService matchService,
            ILogService logService,
            IOptions<AppSettingsKey> appSettingskey)
        {
            this.appSettingskey = appSettingskey.Value;
            this.matchPoolService = matchPoolService;
            this.validationService = validationService;
            this.refundMatchService = refundMatchService;
            this.matchService = matchService;
            this.logService = logService;
        }

        public void Process(Match match)
        {
            var started = true;
            match.IsChecking = true;
            var retryCount = 0;
            matchService.UpdateStatus(match, MatchStatus.Running);
            while (started && retryCount < appSettingskey.RetryCount + 1)
            {
                try
                {
                    validationService.VerifyMatchBeforeExecution(match);
                    var result = refundMatchService.Push(match);
                    ProcessAfterRefund(match, result);
                    started = false;
                }
                catch (AutoRefundException exception)
                {
                    if (!exception.StopLoop)
                    {
                        retryCount = ProcessWhenCatchedException(retryCount, match, exception);
                    }
                    else
                    {
                        ProcessStopLoop(match, exception.Description);
                        started = false;
                    }
                }
            }
        }

        private void ProcessAfterRefund(Match match, Result result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrMsg))
            {
                logService.ActionLog(match, result.ErrMsg);
                matchService.UpdateRemark(match, result.ErrMsg, MatchStatus.Failed);
            }
            matchPoolService.RemoveMatch(match);
        }

        private void ProcessStopLoop(Match match, string error)
        {
            logService.ActionLog(match, error);
            matchService.UpdateRemark(match, error, MatchStatus.Failed);
            matchPoolService.RemoveMatch(match);
        }

        private int ProcessWhenCatchedException(int retryCount, Match match, AutoRefundException exception)
        {
            matchService.UpdateRemark(match, exception.Description, MatchStatus.Running);
            if (retryCount >= appSettingskey.RetryCount)
            {
                logService.ActionLog(match, exception.Description);
                matchPoolService.RemoveAndUpdateStatus(match, MatchStatus.Failed);
            }
            else
            {
                Thread.Sleep(appSettingskey.RetryInterval);
            }

            return retryCount + 1;
        }
    }
}