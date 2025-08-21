using AutoRefundFM.Models;

namespace AutoRefundFM.Services;
public interface IEventCounterService
{
    List<EventCounter> GetEventCountersForBetType(FastMarketResultDetail item, List<EventCounter> eventCounters);
    bool ValidateEventCounterWithBetResult(FastMarketResultDetail item, List<EventCounter> eventCounters);
}

public class EventCounterService : IEventCounterService
{
    public List<EventCounter> GetEventCountersForBetType(FastMarketResultDetail item, List<EventCounter> eventCounters)
    {
        return item.BetType switch
        {
            FastMarketBetType.NextOneMinute => GetEventCountersForNextOneMinute(item, eventCounters),
            FastMarketBetType.NextFiveMinutes => GetEventCountersForNextFiveMinutes(item, eventCounters),
            _ => [],
        };
    }

    private List<EventCounter> GetEventCountersForNextOneMinute(FastMarketResultDetail item, List<EventCounter> eventCounters)
    {
        var eventCounter = eventCounters.FirstOrDefault(ec => ec.Position == item.Position);
        return eventCounter != null ? [eventCounter] : [];
    }

    private List<EventCounter> GetEventCountersForNextFiveMinutes(FastMarketResultDetail item, List<EventCounter> eventCounters)
    {
        const int positionStep = 4;
        var startPosition = item.Position;
        var endPosition = Math.Min(item.Position + positionStep, eventCounters.Count - 1);

        return [.. eventCounters.Where(ec => ec.Position >= startPosition && ec.Position <= endPosition)];
    }

    public bool ValidateEventCounterWithBetResult(FastMarketResultDetail item, List<EventCounter> eventCounters)
    {
        var newEventCounters = GetEventCountersForBetType(item, eventCounters);

        if (newEventCounters.Count == 0)
        {
            return true; // Không cần validate
        }

        switch (item.BetType)
        {
            case FastMarketBetType.NextOneMinute:
                return ValidateNextOneMinute(newEventCounters, item);

            case FastMarketBetType.NextFiveMinutes:
                return ValidateNextFiveMinutes(newEventCounters, item);

            default:
                return true;
        }
    }

    private bool ValidateNextOneMinute(List<EventCounter> eventCounters, FastMarketResultDetail item)
    {
        if (eventCounters.Count == 0)
        {
            return true;
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

    private bool ValidateNextFiveMinutes(List<EventCounter> eventCounters, FastMarketResultDetail item)
    {
        if (!eventCounters.Any())
            return true;

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

    private bool ValidateEventTypeForOneMinute(EventCounter eventCounter, FastMarketResultDetail item, string eventType)
    {
        var eventValue = GetEventValue(eventCounter.EventResultItem, eventType);
        var betValue = GetBetValue(item.ResultItem, eventType);

        return ValidateEventValueForOneMinute(eventValue, betValue);
    }

    private bool ValidateEventTypeForFiveMinutes(List<EventCounter> eventCounters, FastMarketResultDetail item, string eventType)
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
            return false;

        var betValue = GetBetValue(item.ResultItem, eventType);
        var expectedValue = GetExpectedValue(item.ResultItem, eventType);

        return ValidateEventValueForFiveMinutes(totalEvents, expectedValue, betValue);
    }

    private string GetEventValue(EventResultItem eventResultItem, string eventType) => eventType switch
    {
        "Goal" => eventResultItem.Goal ?? "0",
        "Corner" => eventResultItem.Corner ?? "0",
        "FreeKick" => eventResultItem.FreeKick ?? "0",
        "GoalKick" => eventResultItem.GoalKick ?? "0",
        "ThrowIn" => eventResultItem.ThrowIn ?? "0",
        "Penalty" => eventResultItem.Penalty ?? "0",
        _ => "0"
    };

    private bool GetBetValue(ResultItem resultItem, string eventType)
    {
        return eventType switch
        {
            "Goal" => resultItem.Goal,
            "Corner" => resultItem.Corner,
            "FreeKick" => resultItem.FreeKick,
            "GoalKick" => resultItem.GoalKick,
            "ThrowIn" => resultItem.ThrowIn,
            "Penalty" => resultItem.Penalty,
            _ => false
        };
    }

    private int GetExpectedValue(ResultItem resultItem, string eventType)
    {
        // Lấy giá trị mong đợi từ resultItem (có thể cần điều chỉnh theo logic nghiệp vụ)
        return 0; // Placeholder
    }

    private static bool ValidateEventValueForOneMinute(string eventValue, bool betValue)
    {
        const double threshold = 0.5;

        if (!double.TryParse(eventValue, out double numericValue))
        {
            return false;
        }

        return (numericValue > threshold && betValue) || (numericValue < threshold && !betValue);
    }

    private bool ValidateEventValueForFiveMinutes(int totalEvents, int expectedValue, bool betValue)
    {
        return (totalEvents > expectedValue && betValue) || (totalEvents < expectedValue && !betValue);
    }

    private bool IsValidValue(string value)
    {
        return !string.IsNullOrEmpty(value) && int.TryParse(value, out _);
    }
}