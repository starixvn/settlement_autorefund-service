using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.Features.CheckingMarkedTicket;

public class CheckMarkedTicketHandler
{
    private readonly IDynamicRepository repository;
    private readonly AppSettingsKey appSettingsKey;

    public CheckMarkedTicketHandler(IDynamicRepository repository,
        IOptions<AppSettingsKey> options)
    {
        this.repository = repository;
        this.appSettingsKey = options.Value;
    }


    public async Task<bool> Handle(int matchId, string betTypeXml)
    {
        var criteria = new CheckingMarkedTicketCriteria
        {
            MatchId = matchId,
            Results = betTypeXml
        };
        var tasks = appSettingsKey.SiteDbServer
            .Select(server => CheckMarkedTicketAsync(criteria, server));

        var results = await Task.WhenAll(tasks);

        return results.Any(x => x);

    }

    private async Task<bool> CheckMarkedTicketAsync(CheckingMarkedTicketCriteria criteria, string server)
    {
        criteria.DbServer = server;

        var markedTicket = await repository.GetAsync<MarkedTickets>(criteria);

        return markedTicket.IsExistMarkedTicket;
    }
}


public class MarkedTickets
{
    public bool IsExistMarkedTicket { get; set; }

}