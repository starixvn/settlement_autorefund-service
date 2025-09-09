using AutoRefundFM.Models;

namespace AutoRefundFM.Services;
public interface IEventCounterService
{
    List<EventCounter> GetEventCountersForBetType(FastMarketResultDetail item, List<EventCounter> eventCounters);
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

    private static List<EventCounter> GetEventCountersForNextOneMinute(FastMarketResultDetail item, List<EventCounter> eventCounters)
    {
        var eventCounter = eventCounters.FirstOrDefault(ec => ec.Position == item.Position);
        return eventCounter != null ? [eventCounter] : [];
    }

    private static List<EventCounter> GetEventCountersForNextFiveMinutes(FastMarketResultDetail item, List<EventCounter> eventCounters)
    {
        const int positionStep = 4;
        var startPosition = item.Position;
        var endPosition = startPosition + positionStep;

        return [.. eventCounters.Where(ec => ec.Position >= startPosition && ec.Position <= endPosition)];
    }


}