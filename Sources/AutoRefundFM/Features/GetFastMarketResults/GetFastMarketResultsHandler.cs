using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.GetFastMarketResults;

public class GetFastMarketResultsHandler
{
    private readonly IDynamicRepository repository;
    private readonly AppSettingsKey appSettingsKey;


    public GetFastMarketResultsHandler(IDynamicRepository repository, IOptions<AppSettingsKey> options)
    {
        this.repository = repository;
        this.appSettingsKey = options.Value;
    }

    public async Task Handle()
    {
        //var criteria = new GetFastMarketResultsCriteria
        //{
        //    MatchId = 83770848,
        //};

        //var (eventCounters, handicapValues, fastMarketResults, resultDetails) = await repository
        //    .FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(criteria);


        await GetFastMarketTicketDetail(83770848);
    }

    private async Task GetFastMarketTicketDetail(int matchId)
    {
        var criteria = new GetFastMarketTicketDetailCriteria
        {
            MatchId = matchId,
        };

        var servers = appSettingsKey.SiteDbServer.Select(s => GetTicketDetailByServer(criteria, s));

        var results = await Task.WhenAll(servers.ToArray());
        var allBetTypes = results.SelectMany(x => x.betTypes).ToList();
        var ticketCounts = results.Select(x => x.ticketCounts).ToList();

        var totalTicketCount = new TicketCounts
        {
            BetRunningCount = ticketCounts.Sum(x => x.BetRunningCount),
            BetSettledCount = ticketCounts.Sum(x => x.BetSettledCount),
            BetRefundedCount = ticketCounts.Sum(x => x.BetRefundedCount),
            BetCount = ticketCounts.Sum(x => x.BetCount),
        };



    }

    private async Task<(List<FastMarketResultDetail> betTypes, TicketCounts ticketCounts)> GetTicketDetailByServer(
        GetFastMarketTicketDetailCriteria criteria,
        string server)
    {
        criteria.DbServer = server;
        var (ticketCounts, betTypeTickets) = await repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(criteria);

        return (betTypeTickets.ToList(), ticketCounts.FirstOrDefault());
    }
}
