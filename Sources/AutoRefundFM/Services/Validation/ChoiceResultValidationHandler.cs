using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public class ChoiceResultValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var hasInValidResult = validationContext.Session.FastMarketResultDetails
            .Exists(x => !HasChoiceResult(x.BetType, x.ResultItem, x.Result));

        if (hasInValidResult)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.InvalidChoiceResult,
                ErrMsg = "Auto FM Refund failed: Invalid choice result for the specified bet type",
            };
        }

        return await Task.FromResult(new Result());
    }

    private static bool HasChoiceResult(int betType, ResultItem resultItem, int? result = null) => betType switch
    {
        FastMarketBetType.NextOneMinute => HasChoiceResultOfNextOneMinute(resultItem),
        FastMarketBetType.NextFiveMinutes => HasChoiceResultOfNextFiveMinutes(resultItem),
        FastMarketBetType.TimeMachineMinutes => result.HasValue,
        //FastMarketBetType.TimeMachineHdp => !string.IsNullOrEmpty(homeGoal) && !string.IsNullOrEmpty(awayGoal),
        _ => HasChoiceResultOfOther(resultItem),
    };

    private static bool HasChoiceResultOfNextOneMinute(ResultItem resultItem)
        => new List<bool> { resultItem.None, resultItem.Goal, resultItem.Corner, resultItem.FreeKick, resultItem.GoalKick, resultItem.ThrowIn }
        .Exists(x => x);

    private static bool HasChoiceResultOfNextFiveMinutes(ResultItem resultItem)
        => new List<bool> { resultItem.None, resultItem.Goal, resultItem.Corner, resultItem.FreeKick, resultItem.GoalKick, resultItem.ThrowIn, resultItem.Penalty }
        .Exists(x => x);

    private static bool HasChoiceResultOfOther(ResultItem resultItem)
        => new List<bool> { resultItem.None, resultItem.Goal, resultItem.Corner, resultItem.FreeKick, resultItem.GoalKick, resultItem.ThrowIn, resultItem.Bookings, resultItem.Penalty }
        .Exists(x => x);
}
