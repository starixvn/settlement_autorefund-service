using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public class ValidationContext
{
    public SpecialSession Session { get; set; } = new();
    public List<EventCounter> EventCounters { get; set; } = [];
    public string BetTypeRootXml { get; set; } = string.Empty;
    public string EventCountersXml { get; set; } = string.Empty;
}
