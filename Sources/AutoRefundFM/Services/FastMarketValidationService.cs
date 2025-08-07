using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;

namespace AutoRefundFM.Services;


public record ValidateResult(
    bool IsValid,
    string ErrorMessage,
    List<SpecialSession> Sessions,
    List<SpecialSession> InvalidSessions);

public interface IFastMarketValidationService
{
    ValidateResult ValidateSessionsWithCompletedResults(Match match, List<SpecialSession> specialSessions);
    Task<ValidateResult> ValidRunAutoRefund(Match match, List<SpecialSession> specialSessions);
    Task<(bool IsValid, string ErrorMessage)> ValidateEventTicket(Match match, string betTypeRootXml);
    Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml);
}

public class FastMarketValidationService(AutoRefundResultService refundResultService, ScaleOutSettleService scaleOutSettleService,
    ICheckMarkedTicketHandler checkMarkedTicketHandler) : IFastMarketValidationService
{

    public async Task<ValidateResult> ValidRunAutoRefund(Match match, List<SpecialSession> specialSessions)
    {
        var fastMarketResults = specialSessions.SelectMany(x => x.FastMarketResultDetails).ToList();
        var refundResults = await GetRefundResult(match, fastMarketResults);

        var resultsWithExistingRefunds = FindResultsWithExistingRefunds(fastMarketResults, refundResults);

        if (resultsWithExistingRefunds.Count == 0)
        {
            return new ValidateResult(true, string.Empty, specialSessions, []);
        }

        var sessionsWithExistingRefunds = resultsWithExistingRefunds.Select(x => x.SourceId).Distinct().ToList();

        var validSessions = specialSessions
            .Where(x => !sessionsWithExistingRefunds.Contains(x.ResourceId))
            .ToList();

        return new ValidateResult(false, string.Empty, validSessions, []);
    }

    public async Task<(bool IsValid, string ErrorMessage)> ValidateEventTicket(Match match, string betTypeRootXml)
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

    public async Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml)
    {
        return await checkMarkedTicketHandler.Handle(matchId, betTypeRootXml);
    }

    private static List<FastMarketResultDetail> FindResultsWithExistingRefunds(
        List<FastMarketResultDetail> fastMarketResults,
        List<AutoRefundResult> refundResults)
    {
        return fastMarketResults
            .Where(fmResult => refundResults.Exists(refundResult =>
                refundResult.ResourceId == fmResult.BetType.ToString() &&
                refundResult.OtherInfo == fmResult.Position.ToString()))
            .ToList();
    }

    public ValidateResult ValidateSessionsWithCompletedResults(Match match, List<SpecialSession> specialSessions)
    {
        int[] completedStatuses = [Status.CompletedManualResult, Status.CompletedAutoResult];

        var sessionsWithCompletedResults = GetSessionsWithCompletedResults(specialSessions, completedStatuses);


        if (sessionsWithCompletedResults.Count == 0)
        {
            return new ValidateResult(true, string.Empty, specialSessions, []);
        }

        var validSessions = GetValidSessions(specialSessions, sessionsWithCompletedResults);
        var errorMessage = BuildErrorMessage(sessionsWithCompletedResults);


        return new ValidateResult(false, errorMessage, validSessions, sessionsWithCompletedResults);

    }

    private static string BuildErrorMessage(List<SpecialSession> sessionsWithCompletedResults)
    {
        var sessionDetails = string.Join(",", sessionsWithCompletedResults.Select(x => x.ToString()));
        return $"Auto Refund failed: {sessionDetails} due to some intervals have been calculated.";
    }

    private static List<SpecialSession> GetValidSessions(
        List<SpecialSession> allSessions,
        List<SpecialSession> sessionsWithCompletedResults)
    {
        var completedSessionIds = sessionsWithCompletedResults
            .Select(s => s.ResourceId)
            .ToHashSet();

        return allSessions
            .Where(session => !completedSessionIds.Contains(session.ResourceId))
            .ToList();
    }

    private static List<SpecialSession> GetSessionsWithCompletedResults(List<SpecialSession> specialSessions, int[] completedStatuses)
    {
        return [.. specialSessions
            .Where(session => session.FastMarketResultDetails.Any(detail => completedStatuses.Contains(detail.ResultStatus)))
            .Select(session =>
            {
                session.FastMarketResultDetails = [.. session.FastMarketResultDetails.Where(detail => completedStatuses.Contains(detail.ResultStatus))];
                return session;
            })];
    }

    private async Task<List<AutoRefundResult>> GetRefundResult(Match match, List<FastMarketResultDetail> fastMarketResults)
    {
        var getRefundResultsItems = fastMarketResults
            .Select(x => new GetRefundResultsItem(
                match.MatchId,
                x.BetType.ToString(),
                11, // TypeId is 11 for Fast Market
                x.Position.ToString()))
       .ToList();

        var results = await refundResultService.GetResults(getRefundResultsItems);

        return results;
    }
}
