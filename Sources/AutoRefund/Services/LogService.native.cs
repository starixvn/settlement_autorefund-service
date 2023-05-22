using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Services;

public interface LogService
{
    Task ActionLog(Match match, string message);

    void WriteLog(int matchId, string message);
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

    public void WriteLog(int matchId, string message)
    {
        try
        {
            var criteria = new LogDirectlyCriteria
            {
                Extend = matchId,
                Doing = message,
            };

            repository.Execute(criteria);
        }
        catch (Exception e)
        {
            Fanex.Logging.LogService.Error("Fanex.AutoRefund.Service(Write Log Directly): ", e);
        }
    }
}