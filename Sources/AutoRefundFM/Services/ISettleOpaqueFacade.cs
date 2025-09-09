using AutoRefund.Core.Criteria;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;

namespace AutoRefundFM.Services;
public interface SettleOpaqueFacade
{
    Task Refund(SpecialSession session);
}


public class SettleOpaqueFacadeImp(
    IGetFastMarketResultsHandler getFastMarketResultsHandler,
    IPushFastMarketResultHandler pushFastMarketResultHandler,
    ICheckMarkedTicketHandler checkMarkedTicketHandler,
    ISessionPoolService matchPoolService,
    SettlementLogService settlementLogService,
    IFmRefundResultService fmRefundResultService,
    IFastMarketDataTransformationService dataTransformationService,
    IValidationHandler validationHandler) : SettleOpaqueFacade
{
    public async Task Refund(SpecialSession session)
    {
        try
        {
            //await fmRefundResultService.AddOrUpdateSessionResult(session.MatchId, [session], 1);
            var (fmResults, eventCounters) = await getFastMarketResultsHandler.Handle(session);
            session.EventCounters = eventCounters;
            session.FastMarketResultDetails = fmResults;

            var result = await validationHandler.Handle(session);
            if (!result.IsSuccess)
            {
                await HandleSessionError(session, result.ErrMsg);
                return;
            }

            Console.WriteLine(result);


            await ProcessRefund(session, "", "");
        }
        catch (Exception ex)
        {
            //await ExceptionLog(match, ex);
            Console.WriteLine(ex);
        }
        finally
        {
            matchPoolService.RemoveSession(session);
        }
    }

    private async Task HandleSessionError(SpecialSession session, string errorMsg)
    {
        await fmRefundResultService.AddOrUpdateSessionResult(session.MatchId, [session], 3, errorMsg);
        await fmRefundResultService.AddOrUpdateFmResult(
            session.MatchId,
            session.FastMarketResultDetails,
            3,
            errorMsg);
        await settlementLogService.WriteUserLogSettle(new UserLogCriteria
        {
            MatchId = session.MatchId,
            ActionId = 167,
            Remark = errorMsg
        });
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


    public static async Task ExceptionLog(Match match, Exception exception)
    {
        var logMessage = new LogMessage($"Match: {match} - {exception.Message}")
            .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
            .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
            .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
            .ToString();

        await Fanex.Logging.LogService.ErrorAsync(logMessage);
    }

}
