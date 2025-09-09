using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckMarkedTicket;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;

namespace AutoRefundFM.Services;


public interface IFastMarketValidationService
{
    Result ValidateSessionsWithCompletedResults(Match match, List<FastMarketResultDetail> fastMarketResultDetails);
    Task<Result> ValidRunAutoRefund(Match match, List<FastMarketResultDetail> fastMarketResults);
    Task<Result> ValidateEventTicket(Match match, string betTypeRootXml);
    Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml);
    bool HasChoiceResult(
        int betType,
        ResultItem resultItem,
        int? result = null);
}

public class FastMarketValidationService(AutoRefundResultService refundResultService, ScaleOutSettleService scaleOutSettleService,
    ICheckMarkedTicketHandler checkMarkedTicketHandler) : IFastMarketValidationService
{

    public async Task<Result> ValidRunAutoRefund(Match match, List<FastMarketResultDetail> fastMarketResults)
    {
        var refundResults = await GetRefundResult(match, fastMarketResults);

        var resultsWithExistingRefunds = FindResultsWithExistingRefunds(fastMarketResults, refundResults);

        if (resultsWithExistingRefunds.Count == 0)
        {
            return new Result();
        }

        return new Result
        {
            ErrCode = 102,
        };
    }

    public async Task<Result> ValidateEventTicket(Match match, string betTypeRootXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
        };

        var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        validRequest.IsAuto = false;

        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        return new Result
        {
            ErrCode = validationResult.ErrorCode,
            ErrMsg = validationResult.ErrorMessage
        };
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

    public Result ValidateSessionsWithCompletedResults(Match match, List<FastMarketResultDetail> fastMarketResultDetails)
    {
        int[] completedStatuses = [Status.CompletedManualResult, Status.CompletedAutoResult];
        var isExistCompletedResults = fastMarketResultDetails.Exists(detail => completedStatuses.Contains(detail.ResultStatus));

        if (!isExistCompletedResults)
        {
            var errorMessage = BuildErrorMessage([match.SpecialSession]);


            return new Result
            {
                ErrCode = 101,
                ErrMsg = errorMessage,
            };
        }

        return new Result();
    }

    public bool HasChoiceResult(int betType, ResultItem resultItem, int? result = null) => betType switch
    {
        FastMarketBetType.NextOneMinute => HasChoiceResultOfNextOneMinute(resultItem),
        FastMarketBetType.NextFiveMinutes => HasChoiceResultOfNextFiveMinutes(resultItem),
        FastMarketBetType.TimeMachineMinutes => result.HasValue,
        //FastMarketBetType.TimeMachineHdp => !string.IsNullOrEmpty(homeGoal) && !string.IsNullOrEmpty(awayGoal),
        _ => HasChoiceResultOfOther(resultItem),
    };

    private static bool HasChoiceResultOfNextOneMinute(ResultItem resultItem) => resultItem.None ||
               resultItem.Goal ||
               resultItem.Corner ||
               resultItem.FreeKick ||
               resultItem.GoalKick ||
               resultItem.ThrowIn;

    private static bool HasChoiceResultOfNextFiveMinutes(ResultItem resultItem) => resultItem.None ||
               resultItem.Goal ||
               resultItem.Corner ||
               resultItem.FreeKick ||
               resultItem.GoalKick ||
               resultItem.ThrowIn ||
               resultItem.Penalty;

    private static bool HasChoiceResultOfOther(ResultItem resultItem) => resultItem.None ||
               resultItem.Goal ||
               resultItem.Corner ||
               resultItem.FreeKick ||
               resultItem.GoalKick ||
               resultItem.ThrowIn ||
               resultItem.Bookings ||
               resultItem.Penalty;

    private static string BuildErrorMessage(List<SpecialSession> sessionsWithCompletedResults)
    {
        var sessionDetails = string.Join(",", sessionsWithCompletedResults.Select(x => x.ToString()));
        return $"Auto Refund failed: {sessionDetails} due to some intervals have been calculated.";
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
