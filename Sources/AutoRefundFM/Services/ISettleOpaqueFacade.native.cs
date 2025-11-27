using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using Fanex.Data.Repository;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Microsoft.Extensions.Caching.Memory;

namespace AutoRefundFM.Services;
public interface SettleOpaqueFacade
{
    Task Refund(SpecialSession session);
}


public class SettleOpaqueFacadeImp(
    IGetFastMarketResultsHandler getFastMarketResultsHandler,
    IPushFastMarketResultHandler pushFastMarketResultHandler,
    ISessionPoolService matchPoolService,
    SettlementLogService settlementLogService,
    IFmRefundResultService fmRefundResultService,
    IDataTransformationService dataTransformationService,
    IValidationHandler validationHandler,
    IMemoryCache memoryCache,
    IDynamicRepository repository) : SettleOpaqueFacade
{

    private const string RefundTimeCachePrefix = "FM_REFUND_FIRST_";
    private const int AutoRefundFastMarket = 6;

    public async Task Refund(SpecialSession session)
    {
        try
        {
            SetFisrtProcessCache(session);
            var (fmResults, eventCounters) = await getFastMarketResultsHandler.Handle(session);
            var validationContext = CreateContext(session, fmResults, eventCounters);

            var result = await validationHandler.Handle(validationContext);
            if (!result.IsSuccess)
            {
                await HandleSessionError(session, result);
                return;
            }

            await ProcessRefund(session, validationContext.BetTypeRootXml, validationContext.EventCountersXml);
        }
        catch (Exception ex)
        {
            await ExceptionLog(session, ex);
        }
        finally
        {
            matchPoolService.RemoveSession(session);
        }
    }

    private ValidationContext CreateContext(
        SpecialSession specialSession,
        List<FastMarketResultDetail> fastMarketResultDetails,
        List<EventCounter> eventCounters)
    {
        specialSession.FastMarketResultDetails = fastMarketResultDetails;
        var (_, betTypeRootXml) = dataTransformationService.CreateBetTypesRoot(fastMarketResultDetails);
        var eventCountersXml = dataTransformationService.CreateEventCountersXml(eventCounters);

        return new ValidationContext
        {
            Session = specialSession,
            EventCounters = eventCounters,
            BetTypeRootXml = betTypeRootXml,
            EventCountersXml = eventCountersXml
        };
    }

    private async Task HandleSessionError(SpecialSession session, Result result)
    {
        if (!result.MarkFail)
        {
            return;
        }

        await fmRefundResultService.AddOrUpdateSessionResult(session.MatchId, [session], 3, result.ErrMsg);
        var userLog = new UserLogCriteria
        {
            MatchId = session.MatchId,
            ActionId = 167,
            Remark = result.ErrMsg
        };
        await settlementLogService.WriteUserLogSettle(userLog);
        await InsertLetstalkFailMessage(session, result.ErrMsg);
    }

    private async Task InsertLetstalkFailMessage(SpecialSession session, string errorMsg)
    {
        var date = DateTimeOffset.Now;
        var offsetHours = date.Offset.Hours;
        string executeTime = date.ToString("MMM dd, yyyy HH:mm:ss") + $" GMT{offsetHours:+#;-#;+0}";
        var message = $"The match has been executed {errorMsg} at {executeTime}";
        var criteria = new AddNotificationCriteria
        {
            MatchId = session.MatchId,
            MessageContent = message,
            ActionName = $"Auto Refund WaterBreak {session}",
            MessageGroupId = AutoRefundFastMarket, // Auto Refund FM
        };
        await repository.ExecuteAsync(criteria);
    }

    private async Task ProcessRefund(SpecialSession session, string betTypeRootXml, string eventCounterXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = session.MatchId,
            Results = betTypeRootXml,
            EventCounters = eventCounterXml,
        };

        await pushFastMarketResultHandler.Handle(session, criteria);
    }

    private void SetFisrtProcessCache(SpecialSession session)
    {
        session.FirstProcess = memoryCache.GetOrCreate($"{RefundTimeCachePrefix}{session.GetHashCode}", entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(11));
            return DateTimeOffset.Now;
        });
    }


    private static async Task ExceptionLog(SpecialSession session, Exception exception)
    {
        var logMessage = new LogMessage($"Match: {session.MatchId}[{session}] - {exception.Message}")
            .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
            .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
            .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
            .ToString();

        await Fanex.Logging.LogService.ErrorAsync(logMessage);
    }

}
