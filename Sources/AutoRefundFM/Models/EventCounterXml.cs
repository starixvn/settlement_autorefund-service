using System.Xml.Serialization;

namespace AutoRefundFM.Models;


[XmlRoot("EventCounters")]
public class EventCountersRoot
{
    [XmlElement("EventCounter")]
    public List<EventCounterElement> EventCounter { get; set; } = [];
}

public class EventCounterElement
{
    [XmlAttribute]
    public string Id { get; set; } = string.Empty;

    [XmlElement("Item")]
    public List<ItemElement> Items { get; set; } = [];

    // Generalized factory method
    public static EventCounterElement Create(
        List<EventCounter> eventCounters,
        string id,
        Func<EventResultItem, string?> selector)
    {
        var items = eventCounters?
            .Select(ec => new ItemElement
            {
                Position = ec.Position,
                Result = selector(ec.EventResultItem) ?? string.Empty
            })
            .ToList() ?? [];

        return new EventCounterElement
        {
            Id = id,
            Items = items
        };
    }

    public static EventCounterElement GoalResult(List<EventCounter> eventCounters) =>
        Create(eventCounters, EventCounterResource.Goal, eri => eri?.Goal);

    public static EventCounterElement CornerResult(List<EventCounter> eventCounters) =>
        Create(eventCounters, EventCounterResource.Corner, eri => eri?.Corner);

    public static EventCounterElement FreeKickResult(List<EventCounter> eventCounters) =>
        Create(eventCounters, EventCounterResource.FreeKick, eri => eri?.FreeKick);

    public static EventCounterElement GoalKickResult(List<EventCounter> eventCounters) =>
        Create(eventCounters, EventCounterResource.GoalKick, eri => eri?.GoalKick);

    public static EventCounterElement ThrowInResult(List<EventCounter> eventCounters) =>
        Create(eventCounters, EventCounterResource.ThrowIn, eri => eri?.ThrowIn);

    public static EventCounterElement PenaltyResult(List<EventCounter> eventCounters) =>
        Create(eventCounters, EventCounterResource.Penalty, eri => eri?.Penalty);
}