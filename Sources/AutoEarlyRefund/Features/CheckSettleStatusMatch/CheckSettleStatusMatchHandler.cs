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

    public async Task<bool> HandleAsync(Match match)
    {
        var criteria = new CheckSettleStatusMatchCriteria
        {
            MatchId = match.MatchId
        };

        //await repository.ExecuteAsync(criteria);

        //if (criteria.ErrCode == 0 && next != null)
        {
            return await next.HandleAsync(match);
        }

        return criteria.ErrCode == 0;
    }
}
