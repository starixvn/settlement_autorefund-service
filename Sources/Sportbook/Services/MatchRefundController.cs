using AutoRefund.Constants;
using AutoRefund.Models;

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

        private readonly ILogService logService;

        public MatchRefundControllerImp(
            IMatchPoolService matchPoolService,
            IValidationService validationService,
            IRefundMatchService refundMatchService,
            ILogService logService)
        {
            this.matchPoolService = matchPoolService;
            this.validationService = validationService;
            this.refundMatchService = refundMatchService;
            this.logService = logService;

        }

        public void Process(Match match)
        {
            var resultChecking = validationService.VerifyMatchBeforeExecution(match);
            if(resultChecking.ErrCode == 0)
            {
                var result = refundMatchService.Push(match);
                ProcessAfterRefund(match, result);
            } else
            {
                match.NumberOfRetries++;
                if (match.NumberOfRetries == SettingsKey.RetryCount)
                {
                    logService.ActionLog(match, resultChecking.ErrMsg);
                    matchPoolService.Remove(match);
                }
            }
        }

        private void ProcessAfterRefund(Match match, Result result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrMsg))
            {
                logService.ActionLog(match, result.ErrMsg);
            }
            else
            {
                matchPoolService.Remove(match);
            }
        }
    }
}