using AutoRefund.Criteria;
using Fanex.Data.Repository;
using Fanex.Logging;

namespace AutoRefund.Services;
public interface LogDirectlyService
{
    void WriteLog(int matchId, string message);
}

public class LogDirectlyServiceImp : LogDirectlyService
{
    private readonly IDynamicRepository repository;

    public LogDirectlyServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
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
            LogService.Error("Fanex.AutoRefund.Service(Write Log Directly): ", e);
        }
    }
}