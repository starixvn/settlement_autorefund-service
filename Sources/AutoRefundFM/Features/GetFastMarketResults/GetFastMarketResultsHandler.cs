using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetFastMarketResults;

public interface IGetFastMarketResultsHandler
{
    Task<BetTypesRoot> Handle(Match match);
}

public class GetFastMarketResultsHandler : IGetFastMarketResultsHandler
{
    private readonly IDynamicRepository repository;
    private readonly AppSettingsKey appSettingsKey;
    private readonly AutoRefundResultService refundResultService;


    public GetFastMarketResultsHandler(IDynamicRepository repository, IOptions<AppSettingsKey> options, AutoRefundResultService refundResultService)
    {
        this.repository = repository;
        appSettingsKey = options.Value;
        this.refundResultService = refundResultService;
    }

    public async Task<BetTypesRoot> Handle(Match match)
    {
        var fastMarketResults = await GetFastMarketResults(match.MatchId);

        var ticketResults = await GetFastMarketTicketDetail(match.MatchId);
        var newTicketResults = ticketResults
            .Where(item => !fastMarketResults.Any(x => x.Position == item.Position && x.BetType == item.BetType))
            .ToList();

        var combinedResults = fastMarketResults.Concat(newTicketResults)
            .ToList();

        var allFilteredFastMarketResults = match.SpecialSessions
            .SelectMany(specialSession => ApplyFilterByWaterBreak(combinedResults, specialSession))
            .Where(x => x.ResultStatusName.Equals("Running"))
            .ToList();

        var filteredFastMarketResults = await ApllyFilterByRefundResult(match, allFilteredFastMarketResults);

        var betTypesRoot = CreateBetTypesRoot(filteredFastMarketResults);


        var getRefundResultsItems = betTypesRoot.BetType
               .SelectMany(betType => betType.Items.Select(item => new GetRefundResultsItem(
                   match.MatchId,
                   betType.Id.ToString(),
                   11, // assuming TypeId is 11 for Fast Market
                   item.Position.ToString())))
               .ToList();

        var results = await refundResultService.GetResults(getRefundResultsItems);

        return betTypesRoot;
    }

    private async Task<List<FastMarketResultDetail>> ApllyFilterByRefundResult(Match match, List<FastMarketResultDetail> fastMarketResults)
    {
        var getRefundResultsItems = fastMarketResults
       .Select(x => new GetRefundResultsItem(
            match.MatchId,
            x.BetType.ToString(),
            11, // assuming TypeId is 11 for Fast Market
            x.Position.ToString()))
       .ToList();

        var results = await refundResultService.GetResults(getRefundResultsItems);

        return fastMarketResults
            .Where(x => !results.Exists(r => r.ResourceId == x.BetType.ToString() && r.OtherInfo == x.Position.ToString()))
            .ToList();
    }

    private static BetTypesRoot CreateBetTypesRoot(List<FastMarketResultDetail> filteredResults)
    {
        var betTypes = filteredResults.GroupBy(x => x.BetType)
            .OrderBy(x => x.Key)
            .Select(z => new BetTypeElement
            {
                Id = z.Key,
                Items = [.. z.Select(x => new ItemElement
                {
                    Position = x.Position,
                    Result = GetResult(x),

                })]
            }).ToList();

        var betTypesRoot = new BetTypesRoot
        {
            BetType = betTypes
        };

        return betTypesRoot;
    }

    private static IEnumerable<FastMarketResultDetail> ApplyFilterByWaterBreak(
        IEnumerable<FastMarketResultDetail> results,
        SpecialSession specialSession)
    {
        return results
                .Where(item => item.BetType switch
                {
                    FastMarketBetType.TimeMachineHdp or FastMarketBetType.TimeMachineMinutes => false,
                    FastMarketBetType.NextOneMinute or FastMarketBetType.FirstNextOneMinute or
                    FastMarketBetType.NextOneMinuteSetPiece or FastMarketBetType.CombinationNextOneMinute => ApplyFilterOneMinuteWaterBreak(item, specialSession),
                    _ => ApplyFilterFiveMinuteWaterBreak(item, specialSession),
                    //_ => false
                })
                .ToList();
    }

    private static string GetResult(FastMarketResultDetail resultDetail)
    {
        return resultDetail.BetType switch
        {
            FastMarketBetType.TimeMachineMinutes or FastMarketBetType.TimeMachineHdp
                => resultDetail.Result?.ToString() ?? string.Empty,

            FastMarketBetType.NextOneMinuteSetPiece
                => resultDetail.ResultItem.Goal
                    ? SetPiece.CornerFreeKickGoalKickAndPenalty.ToString()
                    : resultDetail.ResultItem.Value.ToString(),

            _ => resultDetail.ResultItem.Value.ToString()
        };
    }

    private static bool ApplyFilterFiveMinuteWaterBreak(FastMarketResultDetail item, SpecialSession waterBreakTime)
    {
        int endMinute = waterBreakTime.StartMinute == waterBreakTime.EndMinute
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

        return fastMarketResults.ToList();
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
