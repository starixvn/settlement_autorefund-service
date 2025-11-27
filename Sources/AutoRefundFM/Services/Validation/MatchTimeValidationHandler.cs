using AutoRefund.Core.Models;

namespace AutoRefundFM.Services.Validation;

public class MatchTimeValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var session = validationContext.Session;

        if (session.StartMinute < 45 && session.EndMinute >= 45)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.InvalidMatchTime,
                ErrMsg = $"Auto FM Refund failed: Invalid time range. Start time ({session.StartMinute}) < 45 but end time ({session.EndMinute}) >= 45",
                MarkFail = true
            };
        }

        return await Task.FromResult(new Result());
    }
}
