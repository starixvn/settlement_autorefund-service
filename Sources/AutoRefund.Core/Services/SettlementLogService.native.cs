using AutoRefund.Core.Criteria;
using Fanex.Data.Repository;

namespace AutoRefund.Core.Services;

public interface SettlementLogService
{
    Task WriteSettlementLog(InsertSettlementLogCriteria insertLogCriteria);

    Task WriteUserLogSettle(int matchId, string message);
}


public class SettlementLogServiceImp(IDynamicRepository repository) : SettlementLogService
{
    public async Task WriteSettlementLog(InsertSettlementLogCriteria insertLogCriteria)
    {
        await repository.ExecuteAsync(insertLogCriteria);
    }

    public async Task WriteUserLogSettle(int matchId, string message)
    {
        await repository.ExecuteAsync(new UserLogWriteLogPreSettlementCriteria
        {
            Extend = matchId,
            Doing = message,
        });
    }
}
