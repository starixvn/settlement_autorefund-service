using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public class ChoiceResultValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(SpecialSession session)
    {
        var hasValidChoice = session.FastMarketResultDetails.Exists(x => !HasChoiceResult(x.BetType, x.ResultItem, x.Result));
        if (hasValidChoice)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.InvalidChoiceResult,
                ErrMsg = "Invalid choice result for the specified bet type",
            };
        }

        return new Result();
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
        => resultItem.None || resultItem.Goal || resultItem.Corner
        || resultItem.FreeKick || resultItem.GoalKick || resultItem.ThrowIn;

    private static bool HasChoiceResultOfNextFiveMinutes(ResultItem resultItem)
        => resultItem.None || resultItem.Goal || resultItem.Corner || resultItem.FreeKick
        || resultItem.GoalKick || resultItem.ThrowIn || resultItem.Penalty;

    private static bool HasChoiceResultOfOther(ResultItem resultItem)
        => resultItem.None || resultItem.Goal || resultItem.Corner || resultItem.FreeKick
        || resultItem.GoalKick || resultItem.ThrowIn || resultItem.Bookings || resultItem.Penalty;
}
