using AutoRefund.Exceptions;
using AutoRefund.Models;

namespace AutoRefund.Services;

public interface IValidationRelateMatchService
{
    void VerifyRelateMatch(MatchChecking matchChecking);
}

public class ValidationRelateMatchServiceImp : IValidationRelateMatchService
{
    public void VerifyRelateMatch(MatchChecking matchChecking)
    {
        if (matchChecking.IsSettled)
        {
            throw new CurrentMatchIsSettledException { StopLoop = true };
        }

        if (!matchChecking.IsMainMatchSettled)
        {
            throw new MainMatchNotCompletedException();
        }

        if (matchChecking.ParentAwayValue == matchChecking.ParentHomeValue)
        {
            throw new MainMatchNotDecisiveException();
        }
    }
}