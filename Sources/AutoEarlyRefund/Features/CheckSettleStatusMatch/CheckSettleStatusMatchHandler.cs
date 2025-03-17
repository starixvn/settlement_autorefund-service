using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckSettleStatusMatch;

public interface ICheckSettleStatusMatchHandler
{
    Task<Result> HandleAsync(Match match);
}

public class CheckSettleStatusMatchHandler : IValidateHandler, ICheckSettleStatusMatchHandler
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

        if (criteria.ErrCode == 0 && next is not null)
        {
            return await next.HandleAsync(match);
        }

        return new Result
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };
    }
}
