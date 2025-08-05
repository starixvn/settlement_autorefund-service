using AutoRefund.Core.Criteria;
using Fanex.Data.Repository;

namespace AutoRefund.Core.Services;

public interface SettlementLogService
{
    Task WriteSettlementLog(InsertSettlementLogCriteria insertLogCriteria);

    Task WriteUserLogPreSettle(int matchId, string message);

    Task WriteUserLogSettle(UserLogCriteria userLogCriteria);
}


public class SettlementLogServiceImp(IDynamicRepository repository) : SettlementLogService
{
    public async Task WriteSettlementLog(InsertSettlementLogCriteria insertLogCriteria)
    {
        await repository.ExecuteAsync(insertLogCriteria);
    }

    public async Task WriteUserLogPreSettle(int matchId, string message)
    {
        await repository.ExecuteAsync(new UserLogWriteLogPreSettlementCriteria
        {
            Extend = matchId,
            Doing = message,
        });
    }

    public async Task WriteUserLogSettle(UserLogCriteria userLogCriteria)
    {
        await repository.ExecuteAsync(userLogCriteria);
    }
}
