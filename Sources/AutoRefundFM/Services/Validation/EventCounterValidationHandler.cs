using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Services.Validation;

public class EventCounterValidationHandler(IOptions<AppSettingsKey> options, IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var z = validationContext.Session.FastMarketResultDetails.Exists(x => !ValidateEventCounterWithBetResult(x));
        var errorMessage = $"Auto FM Refund failed: {validationContext.Session} has result is mismatch with the event counter";
        if (z)
        {
            var isExpired = (DateTimeOffset.Now - validationContext.Session.FirstProcess).TotalMicroseconds > options.Value.RetryProcessScore;
            return new Result
            {
                ErrCode = FastMarketErrorCodes.NoEventCounters,
                ErrMsg = errorMessage,
                MarkFail = isExpired
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

        var eventCounter = eventCounters[0];
        var validationResults = new List<bool>
        {
            ValidateEventTypeForOneMinute(eventCounter, item, "Goal"),
            ValidateEventTypeForOneMinute(eventCounter, item, "Corner"),
            ValidateEventTypeForOneMinute(eventCounter, item, "FreeKick"),
            ValidateEventTypeForOneMinute(eventCounter, item, "GoalKick"),
            ValidateEventTypeForOneMinute(eventCounter, item, "ThrowIn")
        };

        return validationResults.TrueForAll(result => result);
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

        return validationResults.TrueForAll(result => result);
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

    private static readonly Dictionary<string, Func<EventResultItem, string?>> EventValueSelectors = new()
    {
        ["Goal"] = eri => eri.Goal,
        ["Corner"] = eri => eri.Corner,
        ["FreeKick"] = eri => eri.FreeKick,
        ["GoalKick"] = eri => eri.GoalKick,
        ["ThrowIn"] = eri => eri.ThrowIn,
        ["Penalty"] = eri => eri.Penalty
    };

    private static string GetEventValue(EventResultItem eventResultItem, string eventType)
    {
        if (EventValueSelectors.TryGetValue(eventType, out var selector))
        {
            return selector(eventResultItem) ?? "0";
        }
        return "0";
    }

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

    private static readonly Dictionary<string, Func<ResultItemLabel, string?>> HandicapValueSelectors = new()
    {
        ["Goal"] = ril => ril.Goal,
        ["Corner"] = ril => ril.Corner,
        ["FreeKick"] = ril => ril.FreeKick,
        ["GoalKick"] = ril => ril.GoalKick,
        ["ThrowIn"] = ril => ril.ThrowIn,
        ["Penalty"] = ril => ril.Penalty
    };

    private static string GetHandicapValue(ResultItemLabel resultItemLabel, string eventType)
    {
        if (HandicapValueSelectors.TryGetValue(eventType, out var selector))
        {
            return selector(resultItemLabel) ?? "0";
        }
        return "0";
    }

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
