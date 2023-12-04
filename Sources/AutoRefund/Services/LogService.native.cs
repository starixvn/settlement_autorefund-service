namespace AutoRefund.Services;

using Criteria;
using Models;

public interface LogService
{
    Task ActionLog(Match match, string message);

    Task WriteLog(int matchId, string message);

    Task WriteLogResettle(InsertLogCriteria criteria);
}

public class LogServiceImp : LogService
{
    private readonly IDynamicRepository repository;

    public LogServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task ActionLog(Match match, string message)
    {
        var criteria = new LogCriteria(match, message);
        await repository.ExecuteAsync(criteria);
    }

    public async Task WriteLog(int matchId, string message)
    {
        var criteria = new LogDirectlyCriteria
        {
            Extend = matchId,
            Doing = message,
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task WriteLogResettle(InsertLogCriteria criteria)
    {
        await repository.ExecuteAsync(criteria);
    }
}