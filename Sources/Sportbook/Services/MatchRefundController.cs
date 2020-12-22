using AutoRefund.Exceptions;
using AutoRefund.Models;
using Fanex.AutoRefund.Service;

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
            try
            {
                validationService.VerifyMatchBeforeExecution(match);
                var result = refundMatchService.Push(match);

                if (!string.IsNullOrWhiteSpace(result.ErrMsg))
                {
                    logService.ActionLog(match, result.ErrMsg);
                } else
                {
                    matchPoolService.Remove(match);
                }
            }
            catch (AutoRefundException exception)
            {
                match.NumberOfRetries++;
                if (match.NumberOfRetries == AppSettingKeys.RetryCount)
                {
                    logService.ActionLog(match, exception);
                    matchPoolService.Remove(match);
                }
            }
        }
    }
}