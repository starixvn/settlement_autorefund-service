using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckSettleStatusMatch;

public class CheckSettleStatusMatchHandler : IValidateHandler
{
    private readonly IDynamicRepository repository;
    private readonly IValidateHandler next;

    public CheckSettleStatusMatchHandler(IValidateHandler next, IDynamicRepository repository)
    {
        this.repository = repository;
        this.next = next;
    }

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

        if (result.IsSuccess && next is not null)
        {
            return await next.HandleAsync(match);
        }

        return result;
    }
}
