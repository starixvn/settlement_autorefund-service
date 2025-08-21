using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetFastMarketResults;

public interface IGetFastMarketResultsHandler
{
    Task<List<FastMarketResultDetail>> GetSpecialSessions(Match match);
}

public class GetFastMarketResultsHandler(IDynamicRepository repository, IOptions<AppSettingsKey> options) : IGetFastMarketResultsHandler
{
    private readonly AppSettingsKey appSettingsKey = options.Value;

    public async Task<List<FastMarketResultDetail>> GetSpecialSessions(Match match)
    {
        var (evenCounters, fastMarketResults) = await GetFastMarketResults(match.MatchId);

        var ticketResults = await GetFastMarketTicketDetail(match.MatchId);

        var newTicketResults = ticketResults
            .Where(item => !fastMarketResults.Exists(x => x.Position == item.Position && x.BetType == item.BetType));
        var combinedResults = fastMarketResults.Concat(newTicketResults);

        var runnableStatus = new List<int> { 0, 1, 2, 3, Status.CompletedAutoResult, Status.CompletedManualResult };
        var result = match.SpecialSessions
            .SelectMany(session => ApplyFilterByWaterBreak(combinedResults, session)
            .Where(result => runnableStatus.Contains(result.ResultStatus)))
            .OrderBy(result => result.BetType)
            .ToList();

        var filteredEventCounters = ApplyFilterEventCountWaterBreak(evenCounters, match.SpecialSessions[0]);


        foreach (var item in result)
        {
            if (item.BetType == FastMarketBetType.NextOneMinute)
            {
                var eventCounter = filteredEventCounters.FirstOrDefault(x => x.Position == item.Position);
                item.EventCounters = [eventCounter];
            }

            if (item.BetType == FastMarketBetType.NextFiveMinutes)
            {
                item.EventCounters = filteredEventCounters
                      .Where(x => x.Position >= item.Position && x.Position <= item.Position + 4)
                      .ToList();
            }
        }


        return result;
    }

    private string GetEventValue(EventResultItem eventResultItem, string eventType)
    {
        return eventType switch
        {
            "Goal" => eventResultItem.Goal ?? "0",
            "Corner" => eventResultItem.Corner ?? "0",
            "FreeKick" => eventResultItem.FreeKick ?? "0",
            "GoalKick" => eventResultItem.GoalKick ?? "0",
            "ThrowIn" => eventResultItem.ThrowIn ?? "0",
            "Penalty" => eventResultItem.Penalty ?? "0",
            _ => "0"
        };
    }

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

    private bool ValidateEventValue(string eventValue, bool betValue)
    {
        const double threshold = 0.5;

        if (!double.TryParse(eventValue, out double numericValue))
            return false;

        return IsValidValue(eventValue) &&
               ((numericValue > threshold && betValue) || (numericValue < threshold && !betValue));
    }

    private bool IsValidValue(string value) => !string.IsNullOrEmpty(value) && int.TryParse(value, out _);


    private List<EventCounter> GetEventCountersForNextOneMinute(FastMarketResultDetail fastMarketResult, List<EventCounter> eventCounters)
    {
        var eventCounter = eventCounters.FirstOrDefault(ec => ec.Position == fastMarketResult.Position);
        return eventCounter == null ? [] : [eventCounter];
    }

    private List<EventCounter> GetEventCountersForNextFiveMinutes(FastMarketResultDetail fastMarketResult, List<EventCounter> eventCounters)
    {
        const int _positionStep = 4;
        var startPosition = fastMarketResult.Position;
        var endPosition = Math.Min(fastMarketResult.Position + _positionStep, eventCounters.Count - 1);

        return [.. eventCounters.Where(ec => ec.Position >= startPosition && ec.Position <= endPosition)];
    }

    private static List<FastMarketResultDetail> ApplyFilterByWaterBreak(
        IEnumerable<FastMarketResultDetail> results,
        SpecialSession specialSession)
    {
        return [.. results
                .Where(item => item.BetType switch
                {
                    FastMarketBetType.TimeMachineHdp or FastMarketBetType.TimeMachineMinutes => false,
                    FastMarketBetType.NextOneMinute or FastMarketBetType.FirstNextOneMinute or
                    FastMarketBetType.NextOneMinuteSetPiece or FastMarketBetType.CombinationNextOneMinute => ApplyFilterOneMinuteWaterBreak(item, specialSession),
                    _ => ApplyFilterFiveMinuteWaterBreak(item, specialSession),
                    //_ => false
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

    private static List<EventCounter> ApplyFilterEventCountWaterBreak(List<EventCounter> eventCounters, SpecialSession waterBreakTime)
    {
        return eventCounters
            .Where(x => x.Position > waterBreakTime.StartMinute - 5 && x.Position < waterBreakTime.EndMinute + 5)
            .ToList();
    }

    private async Task<(List<EventCounter> eventCounters, List<FastMarketResultDetail>)> GetFastMarketResults(int matchId)
    {
        var criteria = new GetFastMarketResultsCriteria
        {
            MatchId = matchId,
        };

        var (eventCounters, _, fastMarketResults, _) = await repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(criteria);

        return ([.. eventCounters], [.. fastMarketResults]);
    }

    private async Task<List<FastMarketResultDetail>> GetFastMarketTicketDetail(int matchId)
    {
        var criteria = new GetFastMarketTicketDetailCriteria
        {
            MatchId = matchId,
        };

        var servers = appSettingsKey.SiteDbServer.Select(s => GetTicketDetailByServer(criteria, s));

        var results = await Task.WhenAll(servers.ToArray());
        var allTicketResults = results.SelectMany(x => x).ToList();

        var ticketResults = allTicketResults.GroupBy(x => new { x.BetType, x.Position })
            .Select(g => new FastMarketResultDetail
            {
                BetType = g.Key.BetType,
                Position = g.Key.Position,
                Result = g.Sum(x => x.Result ?? 0),
                ResultStatus = g.FirstOrDefault()?.ResultStatus ?? 0,
                TicketStatus = g.FirstOrDefault()?.TicketStatus ?? 0,
                TicketCount = g.Sum(x => x.TicketCount),
                TicketRunning = g.Sum(x => x.TicketRunning),
                TicketMarked = g.Sum(x => x.TicketMarked),
            }).ToList();

        return ticketResults;
    }

    private async Task<IEnumerable<FastMarketResultDetail>> GetTicketDetailByServer(GetFastMarketTicketDetailCriteria criteria, string server)
    {
        criteria.DbServer = server;

        var (_, ticketResults) = await repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(criteria);

        return ticketResults;
    }
}
