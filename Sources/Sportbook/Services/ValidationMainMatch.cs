using AutoRefund.Exceptions;
using AutoRefund.Models;


namespace AutoRefund.Services
{
    public interface IValidationMainMatch
    {
        void VerifyMainMatch(Match match, MatchChecking matchChecking);
    }

    public class ValidationMainMatch : IValidationMainMatch
    {
        public void VerifyMainMatch(Match match, MatchChecking matchChecking)
        {
            if (!matchChecking.IsMainMatchSettled)
            {
                throw new MainMatchNotCompletedException();
            }

            if (matchChecking.finalAwayScore == matchChecking.finalHomeScore)
            {
                throw new MainMatchNotDecisiveException();
            }
        }
    }
}
