using AutoRefundFM.Models;

namespace AutoRefundFM.Services;

public interface IFilterWaterBreakService
{
    List<FastMarketResultDetail> ApplyFilterByWaterBreak(IEnumerable<FastMarketResultDetail> results, SpecialSession specialSession);
    List<EventCounter> ApplyFilterEventCountWaterBreak(List<EventCounter> eventCounters, SpecialSession waterBreakTime);
}

public class FilterWaterBreakService : IFilterWaterBreakService
{
    public List<FastMarketResultDetail> ApplyFilterByWaterBreak(
         IEnumerable<FastMarketResultDetail> results,
         SpecialSession specialSession)
    {
        return [.. results
            .Where(item => item.BetType switch
            {
                FastMarketBetType.TimeMachineHdp or FastMarketBetType.TimeMachineMinutes => false,
                FastMarketBetType.NextOneMinute or FastMarketBetType.FirstNextOneMinute or
                FastMarketBetType.NextOneMinuteSetPiece or FastMarketBetType.CombinationNextOneMinute => ApplyFilterOneMinuteWaterBreak(item, specialSession),
                _ => ApplyFilterFiveMinuteWaterBreak(item, specialSession)
            })];
    }

    private static bool ApplyFilterFiveMinuteWaterBreak(FastMarketResultDetail item, SpecialSession waterBreakTime)
    {
        var endMinute = waterBreakTime.StartMinute == waterBreakTime.EndMinute
            ? waterBreakTime.StartMinute
            : waterBreakTime.EndMinute;

        return item.Position > waterBreakTime.StartMinute - 5 && item.Position <= endMinute;
    }


    private static bool ApplyFilterOneMinuteWaterBreak(FastMarketResultDetail item, SpecialSession waterBreakTime)
    {
        if (waterBreakTime.StartMinute == waterBreakTime.EndMinute)
        {
            return item.Position == waterBreakTime.StartMinute;
        }

        return item.Position >= waterBreakTime.StartMinute && item.Position <= waterBreakTime.EndMinute;
    }

    public List<EventCounter> ApplyFilterEventCountWaterBreak(List<EventCounter> eventCounters, SpecialSession waterBreakTime)
    {
        return [.. eventCounters.Where(x => x.Position > waterBreakTime.StartMinute - 5 && x.Position < waterBreakTime.EndMinute + 5)];
    }
}
