using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public class ExistingRefundsValidationHandler(
    AutoRefundResultService refundResultService, IValidationHandler? next = null) : ValidationHandlerBase(next)
{

    protected async override Task<Result> ProcessAsync(SpecialSession session)
    {
        var refundResults = await GetRefundResult(session);
        var resultsWithExistingRefunds = FindResultsWithExistingRefunds(session.FastMarketResultDetails, refundResults);

        if (resultsWithExistingRefunds.Count > 0)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.ExistingRefundsFound,
                ErrMsg = "Existing refunds found for the specified results"
            };
        }

        return new Result();
    }

    private async Task<List<AutoRefundResult>> GetRefundResult(SpecialSession session)
    {
        var getRefundResultsItems = session.FastMarketResultDetails
            .Select(x => new GetRefundResultsItem(
                session.MatchId,
                x.BetType.ToString(),
                11, // TypeId is 11 for Fast Market
                x.Position.ToString()))
       .ToList();

        var results = await refundResultService.GetResults(getRefundResultsItems);

        return results;
    }

    private static List<FastMarketResultDetail> FindResultsWithExistingRefunds(
        List<FastMarketResultDetail> fastMarketResults,
        List<AutoRefundResult> refundResults)
    {
        return [.. fastMarketResults
            .Where(fmResult => refundResults.Exists(refundResult =>
                refundResult.ResourceId == fmResult.BetType.ToString() &&
                refundResult.OtherInfo == fmResult.Position.ToString()))];
    }
}
