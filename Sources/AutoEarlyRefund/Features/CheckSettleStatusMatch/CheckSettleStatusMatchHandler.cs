using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckSettleStatusMatch;


public class CheckSettleStatusMatchHandler(IValidateHandler next, IDynamicRepository repository) : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {

        var criteria = new CheckSettleStatusMatchCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            IsEarly = true,
            IsAutoRefund = true
        };

        await repository.ExecuteAsync(criteria);

        var result = new Result
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };

        if (result.ErrCode == 0 && next is not null)
        {
            return await next.HandleAsync(match);
        }

        return result;
    }
}
