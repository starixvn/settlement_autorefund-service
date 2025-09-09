using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetFastMarketResults;

public interface IGetFastMarketResultsHandler
{
    Task<(List<FastMarketResultDetail>, List<EventCounter>)> Handle(SpecialSession session);
}

public class GetFastMarketResultsHandler(
    IDynamicRepository repository,
    IOptions<AppSettingsKey> options,
    IFilterWaterBreakService filterWaterBreakService,
    IEventCounterService eventCounterService) : IGetFastMarketResultsHandler
{
    private readonly AppSettingsKey appSettingsKey = options.Value;

    public async Task<(List<FastMarketResultDetail>, List<EventCounter>)> Handle(SpecialSession session)
    {
        var (evenCounters, handicapValue, fastMarketResults) = await GetFastMarketResults(session.MatchId);
        var ticketResults = await GetFastMarketTicketDetail(session.MatchId);

        var newTicketResults = ticketResults
            .Where(item => !fastMarketResults.Exists(x => x.Position == item.Position && x.BetType == item.BetType));
        var combinedResults = fastMarketResults.Concat(newTicketResults);

        var finalResults = FilterResultsByWaterBreakAndStatus(combinedResults, session);
        var finalEventCounters = filterWaterBreakService.ApplyFilterEventCountWaterBreak(evenCounters, session);

        SetResultWithLabelsAndCounters(finalResults, handicapValue, finalEventCounters);


        return (finalResults, finalEventCounters);
    }

    private void SetResultWithLabelsAndCounters(
        List<FastMarketResultDetail> results,
        string handicapValue,
        List<EventCounter> filteredEventCounters)
    {
        foreach (var item in results)
        {
            item.ResultItemLabel = ResultItemLabel.Create(item.BetType, handicapValue);
            item.EventCounters = eventCounterService.GetEventCountersForBetType(item, filteredEventCounters);
        }
    }

    private List<FastMarketResultDetail> FilterResultsByWaterBreakAndStatus(
        IEnumerable<FastMarketResultDetail> combinedResults,
        SpecialSession specialSession)
    {
        List<int> runnableStatus = [0, 1, 2, 3, Status.CompletedAutoResult, Status.CompletedManualResult];

        return [.. filterWaterBreakService
            .ApplyFilterByWaterBreak(combinedResults, specialSession)
            .Where(result => runnableStatus.Contains(result.ResultStatus))];
    }


    private async Task<(List<EventCounter> eventCounters, string handicapValue, List<FastMarketResultDetail>)> GetFastMarketResults(int matchId)
    {
        var criteria = new GetFastMarketResultsCriteria
        {
            MatchId = matchId,
        };

        var (eventCounters, handicapValue, fastMarketResults, _) = await repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(criteria);

        return ([.. eventCounters], handicapValue.First(), [.. fastMarketResults]);
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
