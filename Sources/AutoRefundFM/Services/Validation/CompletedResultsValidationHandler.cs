using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;


public class CompletedResultsValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        int[] completedStatuses = [Status.CompletedManualResult, Status.CompletedAutoResult, Status.RefundResult, Status.RefundDeterminedResult];
        var hasCompletedResults = validationContext.Session.FastMarketResultDetails.Exists(detail =>
            completedStatuses.Contains(detail.ResultStatus));
        var errorMessage = $"Auto Refund failed: {validationContext.Session} due to some intervals have been calculated";

        if (hasCompletedResults)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.NoCompletedResults,
                ErrMsg = errorMessage,
            };
        }

        return await Task.FromResult(new Result());
    }
}