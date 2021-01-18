using AutoRefund.Constants;
using AutoRefund.Models;

namespace AutoRefund.Services
{
    public interface IValidationService
    {
        void VerifyMatchBeforeExecution(Match match);
    }

    public class ValidationServiceImp : IValidationService
    {
        private readonly IValidationSettingMatchService validationSettingMatchService;

        private readonly IValidationTicketMatchService validationTicketMatchService;

        private readonly IValidationRelateMatchService validationRelateMatchService;

        private readonly IMatchService matchService;

        public ValidationServiceImp(
            IValidationSettingMatchService validationSettingMatchService,
            IValidationTicketMatchService validationTicketMatchService,
            IValidationRelateMatchService validationRelateMatchService,
            IMatchService matchService)
        {
            this.validationSettingMatchService = validationSettingMatchService;
            this.validationTicketMatchService = validationTicketMatchService;
            this.validationRelateMatchService = validationRelateMatchService;
            this.matchService = matchService;
        }

        public void VerifyMatchBeforeExecution(Match match)
        {
            var matchChecking = matchService.GetMatchInfo(match);
            if (match.TypeId == MatchType.RefundSingle)
            {
                validationRelateMatchService.VerifyRelateMatch(matchChecking);
                validationSettingMatchService.VerifySettingMatch(matchChecking);
                validationTicketMatchService.VerifyTicketMatch(matchChecking);
            }
            else
            {
                validationRelateMatchService.VerifyChildMatchIsRefudedSingle(matchChecking);
            }
        }
    }
}