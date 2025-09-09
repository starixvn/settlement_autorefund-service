using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;


public class CompletedResultsValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(SpecialSession session)
    {
        int[] completedStatuses = [Status.CompletedManualResult, Status.CompletedAutoResult];
        var hasCompletedResults = session.FastMarketResultDetails.Exists(detail =>
            completedStatuses.Contains(detail.ResultStatus));

        if (!hasCompletedResults)
        {
            var errorMessage = $"Auto Refund failed: {session} due to some intervals have been calculated.";
            return new Result
            {
                ErrCode = FastMarketErrorCodes.NoCompletedResults,
                ErrMsg = errorMessage
            };
        }

        return await Task.FromResult(new Result());
    }
}