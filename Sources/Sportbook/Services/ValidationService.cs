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

        private readonly IValidationMainMatch validationMainMatch;

        private readonly IMatchService matchService;


        public ValidationServiceImp(
            IValidationSettingMatchService validationSettingMatchService,
            IValidationTicketMatchService validationTicketMatchService,
            IValidationMainMatch validationMainMatch,
            IMatchService matchService)
        {
            this.validationSettingMatchService = validationSettingMatchService;
            this.validationTicketMatchService = validationTicketMatchService;
            this.validationMainMatch = validationMainMatch;
            this.matchService = matchService;
        }

        public void VerifyMatchBeforeExecution(Match match)
        {
            var matchChecking = matchService.GetMatchInfo(match);
            validationMainMatch.VerifyMainMatch(match, matchChecking);
            validationSettingMatchService.VerifySettingMatch(matchChecking);
            validationTicketMatchService.VerifyTicketMatch(matchChecking);
        }
    }
}