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
        const string errorMessage = "Auto FM Refund failed: {0} due to some intervals have been calculated at {1}.";

        if (hasCompletedResults)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.NoCompletedResults,
                ErrMsg = string.Format(errorMessage, validationContext.Session, DateTimeOffset.Now),
            };
        }

        return await Task.FromResult(new Result());
    }
}