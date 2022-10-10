using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;

namespace AutoRefund.Services;

public interface ILogService
{
    Task ActionLog(Match match, string message);
}

public class LogServiceImp : ILogService
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
}