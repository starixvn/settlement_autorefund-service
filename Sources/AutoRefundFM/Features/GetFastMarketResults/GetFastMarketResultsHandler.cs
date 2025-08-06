using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetFastMarketResults;

public interface IGetFastMarketResultsHandler
{
    Task<List<SpecialSession>> GetSpecialSessions(Match match);
}

public class GetFastMarketResultsHandler(IDynamicRepository repository, IOptions<AppSettingsKey> options) : IGetFastMarketResultsHandler
{
    private readonly AppSettingsKey appSettingsKey = options.Value;

    public async Task<List<SpecialSession>> GetSpecialSessions(Match match)
    {
        var fastMarketResults = await GetFastMarketResults(match.MatchId);

        var ticketResults = await GetFastMarketTicketDetail(match.MatchId);

        var newTicketResults = ticketResults
            .Where(item => !fastMarketResults.Exists(x => x.Position == item.Position && x.BetType == item.BetType));
        var combinedResults = fastMarketResults.Concat(newTicketResults);

        var runnableStatus = new List<int> { 0, 1, 2, 3, Status.CompletedAutoResult, Status.CompletedManualResult };
        var result = match.SpecialSessions
            .Select(session =>
            {
                session.FastMarketResultDetails = [.. ApplyFilterByWaterBreak(combinedResults, session)
                    .Where(result => runnableStatus.Contains(result.ResultStatus))
                    .Select(result =>
                    {
                        result.SourceId = session.ResourceId;
                        return result;
                    })];
                return session;
            }).ToList();

        return result;
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

    private async Task<List<FastMarketResultDetail>> GetFastMarketResults(int matchId)
    {
        var criteria = new GetFastMarketResultsCriteria
        {
            MatchId = matchId,
        };

        var (_, _, fastMarketResults, _) = await repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(criteria);

        return [.. fastMarketResults];
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
