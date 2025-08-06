using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;

namespace AutoRefundFM.Services;
public interface SettleOpaqueFacade
{
    Task Refund(Match match);
}


public class SettleOpaqueFacadeImp(
    IGetFastMarketResultsHandler getFastMarketResultsHandler,
    ScaleOutSettleService scaleOutSettleService,
    XmlConvertService xmlConvertService,
    IPushFastMarketResultHandler pushFastMarketResultHandler,
    ICheckMarkedTicketHandler checkMarkedTicketHandler,
    IMatchPoolService matchPoolService,
    SettlementLogService settlementLogService,
    IValidateSessionService validateResultDetailService,
    IFmRefundResultService fmRefundResultService) : SettleOpaqueFacade
{
    public async Task Refund(Match match)
    {
        try
        {
            await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, match.SpecialSessions, 1);
            var sessionResults = await getFastMarketResultsHandler.GetSpecialSessions(match);
            // validate result
            var validateCompletedResult = validateResultDetailService.ValidateSessionsWithCompletedResults(match, sessionResults);
            if (!validateCompletedResult.IsValid && validateCompletedResult.InvalidSessions.Count > 0)
            {
                await settlementLogService.WriteUserLogSettle(new UserLogCriteria
                {
                    MatchId = match.MatchId,
                    ActionId = 167,
                    Remark = validateCompletedResult.ErrorMessage
                });

                await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, validateCompletedResult.InvalidSessions, 3);
                await fmRefundResultService.AddOrUpdateBetTypeResult(
                    match.MatchId,
                    validateCompletedResult.InvalidSessions.SelectMany(x => x.FastMarketResultDetails).ToList(),
                    3,
                    validateCompletedResult.ErrorMessage);
            }

            if (validateCompletedResult.Sessions.Count == 0)
            {
                return;
            }

            var validRunAutoRefund = await validateResultDetailService.ValidRunAutoRefund(match, validateCompletedResult.Sessions);

            if (validRunAutoRefund.Sessions.Count == 0)
            {
                await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, validRunAutoRefund.InvalidSessions, 2);
                return;
            }

            var validSessions = validRunAutoRefund.Sessions;
            var validFm = validSessions.SelectMany(x => x.FastMarketResultDetails).ToList();
            var betTypeRoot = CreateBetTypesRoot([.. validFm]);

            if (betTypeRoot.BetType.Count == 0)
            {
                await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, validSessions, 2);
                return;
            }

            var (betTypeRootXml, resourceXml) = PrepareRefundData(betTypeRoot);

            var validationResult = await ValidateRefundRequest(match, betTypeRootXml);
            if (!validationResult.IsValid)
            {
                await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, validSessions, 3);
                await fmRefundResultService.AddOrUpdateBetTypeResult(
                    match.MatchId,
                    validFm,
                    3,
                    validationResult.ErrorMessage);
                await settlementLogService.WriteUserLogSettle(new UserLogCriteria
                {
                    MatchId = match.MatchId,
                    ActionId = 167,
                    Remark = validationResult.ErrorMessage
                });

                return;
            }

            if (await HasMarkedTickets(match.MatchId, betTypeRootXml))
            {
                var errorMessage = "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again.";
                await fmRefundResultService.AddOrUpdateSessionResult(match.MatchId, validSessions, 3);
                await fmRefundResultService.AddOrUpdateBetTypeResult(
                    match.MatchId,
                    validFm,
                    3,
                    errorMessage);
                await settlementLogService.WriteUserLogSettle(new UserLogCriteria
                {
                    MatchId = match.MatchId,
                    ActionId = 167,
                    Remark = errorMessage
                });

                return;
            }

            await ProcessRefund(new Match { MatchId = match.MatchId, SpecialSessions = validSessions }, betTypeRootXml, resourceXml);
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


    private (string betTypeRootXml, string resourceXml) PrepareRefundData(BetTypesRoot betTypeRoot)
    {
        var betTypeRootXml = xmlConvertService.Serialize(betTypeRoot);

        return (betTypeRootXml, resourceXml);
    }



    private async Task<(bool IsValid, string ErrorMessage)> ValidateRefundRequest(Match match, string betTypeRootXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
        };

        var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        validRequest.IsAuto = false;

        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        return validationResult.ErrorCode > 0
            ? (false, validationResult.ErrorMessage)
            : (true, string.Empty);
    }

    private async Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml)
    {
        return await checkMarkedTicketHandler.Handle(matchId, betTypeRootXml);
    }

    private async Task ProcessRefund(Match match, string betTypeRootXml, string resourceXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
        };

        await pushFastMarketResultHandler.Handle(match, criteria, resourceXml);
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

    private string CreateResourceXml(List<ResourceItem> resourceItems)
    {
        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };

        var selectedResources = xmlConvertService.Serialize(resourceRoot);

        return selectedResources;
    }

    private static List<ResourceItem> CreateResourceItemsFromResult(List<FastMarketResultDetail> fastMarketResults)
    {
        return fastMarketResults.Select(x => new ResourceItem
        {
            ResourceId = x.BetType.ToString(),
            HomeValue = $"{x.Position}",
            AwayValue = $"{x.Result}",
            OtherInfo = x.SourceId,
        }).ToList();
    }

    private static BetTypesRoot CreateBetTypesRoot(List<FastMarketResultDetail> filteredResults)
    {
        var betTypes = filteredResults.GroupBy(g => g.BetType)
            .OrderBy(g => g.Key)
            .Select(g => new BetTypeElement
            {
                Id = g.Key,
                Items = [.. g.Select(x => new ItemElement
                {
                    Position = x.Position,
                    Result = GetResult(x),

                })]
            }).ToList();

        var betTypesRoot = new BetTypesRoot
        {
            BetType = betTypes ?? []
        };

        return betTypesRoot;
    }

    private static string GetResult(FastMarketResultDetail resultDetail)
    {
        return resultDetail.BetType switch
        {
            FastMarketBetType.TimeMachineMinutes or FastMarketBetType.TimeMachineHdp
                => resultDetail.Result?.ToString() ?? string.Empty,

            FastMarketBetType.NextOneMinuteSetPiece
                => resultDetail.ResultItem.Goal
                    ? SetPiece.CornerFreeKickGoalKickAndPenalty.ToString()
                    : resultDetail.ResultItem.Value.ToString(),

            _ => resultDetail.ResultItem.Value.ToString()
        };
    }

}
