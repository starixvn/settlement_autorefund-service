using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Logging;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Features.CheckSettleStatusMatch;

public interface ICheckSettleStatusMatchHandler
{
    Task<Result> HandleAsync(Match match);
}

public class CheckSettleStatusMatchHandler : IValidateHandler, ICheckSettleStatusMatchHandler
{
    private readonly IDynamicRepository repository;
    private readonly IValidateHandler next;
    private readonly ILogger<CheckSettleStatusMatchHandler> logger;

    public CheckSettleStatusMatchHandler(IValidateHandler next, IDynamicRepository repository, ILogger<CheckSettleStatusMatchHandler> logger)
    {
        this.repository = repository;
        this.next = next;
        this.logger = logger;
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

        Logger.Log.Info($"CheckSettleStatusMatchHandler: {match} {result}");
        logger.LogInformation($"CheckSettleStatusMatchHandler: {match} {result}");

        if (result.ErrCode == 0 && next is not null)
        {
            return await next.HandleAsync(match);
        }

        return result;
    }
}
