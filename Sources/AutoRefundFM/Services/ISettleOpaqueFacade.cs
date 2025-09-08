using AutoRefund.Core.Criteria;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;

namespace AutoRefundFM.Services;
public interface SettleOpaqueFacade
{
    Task Refund(Match match);
}


public class SettleOpaqueFacadeImp(
    IGetFastMarketResultsHandler getFastMarketResultsHandler,
    IPushFastMarketResultHandler pushFastMarketResultHandler,
    ICheckMarkedTicketHandler checkMarkedTicketHandler,
    IMatchPoolService matchPoolService,
    SettlementLogService settlementLogService,
    IFastMarketValidationService fastMarketValidationService,
    IFmRefundResultService fmRefundResultService,
    IFastMarketDataTransformationService dataTransformationService,
    IEventCounterService eventCounterService) : SettleOpaqueFacade
{
    public async Task Refund(Match match)
    {
        try
        {
            //await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, match.SpecialSessions, 1);
            var (fmResults, eventCounters) = await getFastMarketResultsHandler.Handle(match);
            //var x = fmResults.Exists(x => !eventCounterService.ValidateEventCounterWithBetResult(x));
            //var errorX = "Has result is not consistent with the event counter";

            //var y = fmResults.Exists(x => fastMarketValidationService.HasChoiceResult(x.BetType, x.ResultItem, x.Result));

            //var errorY = "Has result with choice but no result is set";


            //var validateCompletedResult = fastMarketValidationService.ValidateSessionsWithCompletedResults(match, fmResults);
            //if (!validateCompletedResult.IsSuccess)
            //{
            //    await HandleMatchError(match, fmResults, validateCompletedResult.ErrMsg);
            //    return;
            //}

            //var validRunAutoRefund = await fastMarketValidationService.ValidRunAutoRefund(match, fmResults);
            //if (!validateCompletedResult.IsSuccess)
            //{
            //    await fmRefundResultService.AddOrUpdateSessionResult(match, 2);//
            //    return;
            //}

            var (_, betTypeRootXml) = dataTransformationService.CreateBetTypesRoot(fmResults);
            var eventCountersXml = dataTransformationService.CreateEventCountersXml(eventCounters);

            //var validationResult = await fastMarketValidationService.ValidateEventTicket(match, betTypeRootXml);
            //if (!validationResult.IsSuccess)
            //{
            //    await HandleMatchError(match, fmResults, validationResult.ErrMsg);
            //    return;
            //}

            //if (await HasMarkedTickets(match.MatchId, betTypeRootXml))
            //{
            //    const string errorMessage = "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again.";
            //    await HandleMatchError(match, fmResults, errorMessage);
            //    return;
            //}

            await ProcessRefund(new Match { MatchId = match.MatchId }, betTypeRootXml, eventCountersXml);
        }
        catch (Exception ex)
        {
            await ExceptionLog(match, ex);
        }
        finally
        {
            matchPoolService.RemoveMatch(match);
        }
    }

    private async Task HandleMatchError(Match match, List<FastMarketResultDetail> fastMarketResultDetails, string errorMsg)
    {
        var sessions = match.SpecialSessions;

        await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, sessions, 3, errorMsg);
        await fmRefundResultService.AddOrUpdateBetTypeResult(
            match.MatchId,
            fastMarketResultDetails,
            3,
            errorMsg);
        await settlementLogService.WriteUserLogSettle(new UserLogCriteria
        {
            MatchId = match.MatchId,
            ActionId = 167,
            Remark = errorMsg
        });
    }

    private async Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml)
        => await checkMarkedTicketHandler.Handle(matchId, betTypeRootXml);

    private async Task ProcessRefund(Match match, string betTypeRootXml, string eventCounterXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
            EventCounters = eventCounterXml,
        };

        await pushFastMarketResultHandler.Handle(match, criteria);
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
