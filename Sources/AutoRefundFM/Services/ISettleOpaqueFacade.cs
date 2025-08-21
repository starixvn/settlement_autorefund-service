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
    IFastMarketDataTransformationService dataTransformationService) : SettleOpaqueFacade
{
    public async Task Refund(Match match)
    {
        try
        {
            //await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, match.SpecialSessions, 1);
            var sessionResults = await getFastMarketResultsHandler.GetSpecialSessions(match);

            //var validateCompletedResult = fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults);
            //if (!validateCompletedResult.IsValid && validateCompletedResult.InvalidSessions.Count > 0)
            //{
            //    await HandleMatchError(match.MatchId, validateCompletedResult.InvalidSessions, validateCompletedResult.ErrorMessage);
            //}

            //if (validateCompletedResult.Sessions.Count == 0)
            //{
            //    return;
            //}

            //var validRunAutoRefund = await fastMarketValidationService.ValidRunAutoRefund(match, validateCompletedResult.Sessions);
            //if (validRunAutoRefund.Sessions.Count == 0)
            //{
            //    await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, validRunAutoRefund.InvalidSessions, 2);
            //    return;
            //}

            //var validSessions = validRunAutoRefund.Sessions;
            //var (_, betTypeRootXml) = dataTransformationService.CreateBetTypesRoot([.. validSessions]);

            //var validationResult = await fastMarketValidationService.ValidateEventTicket(match, betTypeRootXml);
            //if (!validationResult.IsValid)
            //{
            //    await HandleMatchError(match.MatchId, validSessions, validationResult.ErrorMessage);
            //    return;
            //}

            //if (await HasMarkedTickets(match.MatchId, betTypeRootXml))
            //{
            //    const string errorMessage = "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again.";
            //    await HandleMatchError(match.MatchId, validSessions, errorMessage);
            //    return;
            //}

            //await ProcessRefund(new Match { MatchId = match.MatchId, SpecialSessions = validSessions }, betTypeRootXml);
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

    private async Task HandleMatchError(int matchId, List<SpecialSession> sessions, string errorMsg)
    {
        var fastMarketResults = sessions.SelectMany(x => x.FastMarketResultDetails).ToList();

        await fmRefundResultService.AddOrUpdateSessionResult(matchId, sessions, 3, errorMsg);
        await fmRefundResultService.AddOrUpdateBetTypeResult(
            matchId,
            fastMarketResults,
            3,
            errorMsg);
        await settlementLogService.WriteUserLogSettle(new UserLogCriteria
        {
            MatchId = matchId,
            ActionId = 167,
            Remark = errorMsg
        });
    }

    private async Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml)
        => await checkMarkedTicketHandler.Handle(matchId, betTypeRootXml);

    private async Task ProcessRefund(Match match, string betTypeRootXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
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
