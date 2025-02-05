using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckMarkedTicket;

public class CheckingMarkedTicketHandler : IValidateHandler
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly IValidateHandler next;

    public CheckingMarkedTicketHandler(IValidateHandler next, IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.appSettingsKey = appSettingsKey;
        this.next = next;
    }

    //public CheckingMarkedTicketHandler(IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey)
    //{
    //    this.repository = repository;
    //    this.appSettingsKey = appSettingsKey;
    //}

    public async Task<bool> HandleAsync(Match match)
    {
        var criteria = new CheckingMarkedTicketCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND"
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckMarkedTicket(s, criteria));

        var results = await Task.WhenAll(tasks.ToArray());

        //if (results.Any(x => !x) && next != null)
        //{
        return await next.HandleAsync(match);
        //}

        return results.Any(x => x);
    }

    private async Task<bool> CheckMarkedTicket(string s, CheckingMarkedTicketCriteria criteria)
    {
        criteria.DbServer = s;
        await repository.ExecuteAsync(criteria);

        return criteria.IsExistMarkedTicket;
    }
}
