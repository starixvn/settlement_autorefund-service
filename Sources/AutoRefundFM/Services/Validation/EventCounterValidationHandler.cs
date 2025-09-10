using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public class EventCounterValidationHandler(IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var z = validationContext.Session.FastMarketResultDetails.Exists(x => !ValidateEventCounterWithBetResult(x));
        if (z)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.NoEventCounters,
                ErrMsg = "Has result is not consistent with the event counter",
            };
        }

        return await Task.FromResult(new Result());
    }

    private static bool ValidateEventCounterWithBetResult(FastMarketResultDetail item)
    {
        return item.BetType switch
        {
            FastMarketBetType.NextOneMinute => ValidateNextOneMinute(item.EventCounters, item),
            FastMarketBetType.NextFiveMinutes => ValidateNextFiveMinutes(item.EventCounters, item),
            _ => true,
        };
    }

    private static bool ValidateNextOneMinute(List<EventCounter> eventCounters, FastMarketResultDetail item)
    {
        if (eventCounters.Count == 0)
        {
            return false;
        }

        var eventCounter = eventCounters.First();
        var validationResults = new List<bool>
        {
            ValidateEventTypeForOneMinute(eventCounter, item, "Goal"),
            ValidateEventTypeForOneMinute(eventCounter, item, "Corner"),
            ValidateEventTypeForOneMinute(eventCounter, item, "FreeKick"),
            ValidateEventTypeForOneMinute(eventCounter, item, "GoalKick"),
            ValidateEventTypeForOneMinute(eventCounter, item, "ThrowIn")
        };

        return validationResults.All(result => result);
    }

    private static bool ValidateNextFiveMinutes(List<EventCounter> eventCounters, FastMarketResultDetail item)
    {
        var validationResults = new List<bool>
        {
            ValidateEventTypeForFiveMinutes(eventCounters, item, "Goal"),
            ValidateEventTypeForFiveMinutes(eventCounters, item, "Corner"),
            ValidateEventTypeForFiveMinutes(eventCounters, item, "FreeKick"),
            ValidateEventTypeForFiveMinutes(eventCounters, item, "GoalKick"),
            ValidateEventTypeForFiveMinutes(eventCounters, item, "ThrowIn"),
            ValidateEventTypeForFiveMinutes(eventCounters, item, "Penalty")
        };

        return validationResults.All(result => result);
    }

    private static bool ValidateEventTypeForOneMinute(EventCounter eventCounter, FastMarketResultDetail item, string eventType)
    {
        var eventValue = GetEventValue(eventCounter.EventResultItem, eventType);
        var betValue = GetBetValue(item.ResultItem, eventType);

        return ValidateEventValueForOneMinute(eventValue, betValue);
    }

    private static bool ValidateEventTypeForFiveMinutes(List<EventCounter> eventCounters, FastMarketResultDetail item, string eventType)
    {
        int totalEvents = 0;
        bool isValid = true;

        foreach (var eventCounter in eventCounters)
        {
            var eventValue = GetEventValue(eventCounter.EventResultItem, eventType);

            if (!IsValidValue(eventValue))
            {
                isValid = false;
                break;
            }

            totalEvents += int.Parse(eventValue);
        }

        if (!isValid)
        {
            return false;
        }


        var betValue = GetBetValue(item.ResultItem, eventType);
        var expectedValue = GetExpectedValue(item, eventType);

        return ValidateEventValueForFiveMinutes(totalEvents, expectedValue, betValue);
    }

    private static string GetEventValue(EventResultItem eventResultItem, string eventType) => eventType switch
    {
        "Goal" => eventResultItem.Goal ?? "0",
        "Corner" => eventResultItem.Corner ?? "0",
        "FreeKick" => eventResultItem.FreeKick ?? "0",
        "GoalKick" => eventResultItem.GoalKick ?? "0",
        "ThrowIn" => eventResultItem.ThrowIn ?? "0",
        "Penalty" => eventResultItem.Penalty ?? "0",
        _ => "0"
    };

    private static bool GetBetValue(ResultItem resultItem, string eventType) => eventType switch
    {
        "Goal" => resultItem.Goal,
        "Corner" => resultItem.Corner,
        "FreeKick" => resultItem.FreeKick,
        "GoalKick" => resultItem.GoalKick,
        "ThrowIn" => resultItem.ThrowIn,
        "Penalty" => resultItem.Penalty,
        _ => false
    };

    private static double GetExpectedValue(FastMarketResultDetail resultItem, string eventType)
    {
        var value = GetHandicapValue(resultItem.ResultItemLabel, eventType);

        return double.Parse(value);
    }

    private static string GetHandicapValue(ResultItemLabel resultItemLabel, string eventType) => eventType switch
    {
        "Goal" => resultItemLabel.Goal ?? "0",
        "Corner" => resultItemLabel.Corner ?? "0",
        "FreeKick" => resultItemLabel.FreeKick ?? "0",
        "GoalKick" => resultItemLabel.GoalKick ?? "0",
        "ThrowIn" => resultItemLabel.ThrowIn ?? "0",
        "Penalty" => resultItemLabel.Penalty ?? "0",
        _ => "0"
    };

    private static bool ValidateEventValueForOneMinute(string eventValue, bool betValue)
    {
        const double threshold = 0.5;

        if (!double.TryParse(eventValue, out double numericEventValue))
        {
            return false;
        }

        return (numericEventValue > threshold && betValue) || (numericEventValue < threshold && !betValue);
    }

    private static bool ValidateEventValueForFiveMinutes(double totalScoreEvents, double expectedValue, bool betValue)
        => (totalScoreEvents > expectedValue && betValue) || (totalScoreEvents < expectedValue && !betValue);

    private static bool IsValidValue(string value)
        => !string.IsNullOrEmpty(value) && int.TryParse(value, out _);
}
