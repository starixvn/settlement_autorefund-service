using AutoRefund.Core.Models;

namespace AutoRefundFM.Services.Validation;

public class MatchTimeValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var session = validationContext.Session;

        if (!int.TryParse(session.HomeValue, out var startTime))
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.InvalidMatchTime,
                ErrMsg = $"Auto FM Refund failed: Invalid start time '{session.HomeValue}'",
            };
        }

        if (!int.TryParse(session.AwayValue, out var endTime))
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.InvalidMatchTime,
                ErrMsg = $"Auto FM Refund failed: Invalid end time '{session.AwayValue}'",
                MarkFail = true
            };
        }

        if (startTime < 45 && endTime > 45)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.InvalidMatchTime,
                ErrMsg = $"Auto FM Refund failed: Invalid time range. Start time ({startTime}) < 45 but end time ({endTime}) > 45",
                MarkFail = true
            };
        }

        return await Task.FromResult(new Result());
    }
}
