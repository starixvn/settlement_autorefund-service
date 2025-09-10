using AutoRefundFM.Models;

namespace AutoRefundFM.Services;

public interface IFilterWaterBreakService
{
    List<FastMarketResultDetail> ApplyFilterByWaterBreak(IEnumerable<FastMarketResultDetail> results, SpecialSession session);
    List<EventCounter> ApplyFilterEventCountersByWaterBreak(List<EventCounter> eventCounters, SpecialSession session);
}

public class FilterWaterBreakService : IFilterWaterBreakService
{
    public List<FastMarketResultDetail> ApplyFilterByWaterBreak(
         IEnumerable<FastMarketResultDetail> results,
         SpecialSession session)
    {
        return [.. results
            .Where(item => item.BetType switch
            {
                FastMarketBetType.TimeMachineHdp or FastMarketBetType.TimeMachineMinutes => false,
                FastMarketBetType.NextOneMinute or FastMarketBetType.FirstNextOneMinute or
                FastMarketBetType.NextOneMinuteSetPiece or FastMarketBetType.CombinationNextOneMinute
                    => ApplyFilterOneMinuteWaterBreak(item, session),
                _ => ApplyFilterFiveMinuteWaterBreak(item, session)
            })];
    }

    private static bool ApplyFilterFiveMinuteWaterBreak(FastMarketResultDetail item, SpecialSession session)
    {
        var endMinute = session.StartMinute == session.EndMinute
            ? session.StartMinute
            : session.EndMinute;

        return item.Position > session.StartMinute - 5 && item.Position <= endMinute;
    }


    private static bool ApplyFilterOneMinuteWaterBreak(FastMarketResultDetail item, SpecialSession session)
    {
        if (session.StartMinute == session.EndMinute)
        {
            return item.Position == session.StartMinute;
        }

        return item.Position >= session.StartMinute && item.Position <= session.EndMinute;
    }

    public List<EventCounter> ApplyFilterEventCountersByWaterBreak(List<EventCounter> eventCounters, SpecialSession session)
    {
        return [.. eventCounters.Where(x => x.Position > session.StartMinute - 5 && x.Position < session.EndMinute + 5)];
    }
}
