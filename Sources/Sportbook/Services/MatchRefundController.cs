using AutoRefund.Constants;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
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
            var watch = Stopwatch.StartNew();
            match.IsChecking = true;
            var continueLoop = true;
            matchService.UpdateStatus(match, MatchStatus.Running);
            while (continueLoop)
            {
                try
                {
                    validationService.VerifyMatchBeforeExecution(match);
                    var result = refundMatchService.Push(match);
                    ProcessAfterRefund(match, result);
                    continueLoop = false;
                }
                catch (AutoRefundException exception)
                {
                    if (!exception.StopLoop)
                    {
                        continueLoop = ProcessWhenCatchedException(watch, match, exception);
                    }
                    else
                    {
                        logService.ActionLog(match, exception.Description);
                        matchService.UpdateRemark(match, exception.Description, MatchStatus.Failed);
                        matchPoolService.RemoveMatch(match);
                        continueLoop = !exception.StopLoop;
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

        private bool ProcessWhenCatchedException(Stopwatch watch, Match match, AutoRefundException exception)
        {
            matchService.UpdateRemark(match, exception.Description, MatchStatus.Running);
            if (watch.ElapsedMilliseconds > appSettingskey.MaxTimeRetry)
            {
                logService.ActionLog(match, exception.Description);
                matchPoolService.RemoveAndUpdateStatus(match, MatchStatus.Failed);
                return false;
            }
            else
            {
                Thread.Sleep(appSettingskey.RetryInterval);
            }
            return true;
        }
    }
}