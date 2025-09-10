using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;


public class CompletedResultsValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        int[] completedStatuses = [Status.CompletedManualResult, Status.CompletedAutoResult];
        var hasCompletedResults = validationContext.Session.FastMarketResultDetails.Exists(detail =>
            completedStatuses.Contains(detail.ResultStatus));

        if (hasCompletedResults)
        {
            var errorMessage = $"Auto FM Refund failed: {validationContext.Session} due to some intervals have been calculated.";
            return new Result
            {
                ErrCode = FastMarketErrorCodes.NoCompletedResults,
                ErrMsg = errorMessage
            };
        }

        return await Task.FromResult(new Result());
    }
}