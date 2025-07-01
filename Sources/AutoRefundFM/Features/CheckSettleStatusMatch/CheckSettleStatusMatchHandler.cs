using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Data.Repository;

namespace AutoRefundFM.Features.CheckSettleStatusMatch;

public class CheckSettleStatusMatchHandler(
    IValidateHandler next,
    IDynamicRepository repository)
{
    public async Task<Result> HandleAsync(Match match)
    {
        var criteria = new CheckSettleStatusMatchCriteria
        {
            MatchId = match.MatchId
        };

        await repository.ExecuteAsync(criteria);

        var result = new Result
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };

        if (result.IsSuccess && next is not null)
        {
            return await next.HandleAsync(match);
        }

        return result;
    }
}