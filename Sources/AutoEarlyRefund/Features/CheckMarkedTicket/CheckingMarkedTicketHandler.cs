using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckMarkedTicket;

public class CheckingMarkedTicketHandler(
    IDynamicRepository repository,
    IOptions<AppSettingsKey> appSettingsKey,
    IValidateHandler? next)
    : IValidateHandler
{
    public async Task<bool> HandleAsync(Match match)
    {
        var criteria = new CheckingMarkedTicketCriteria
        {
            MatchId = match.MatchId
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckMarkedTicket(s, criteria));

        var results = (await Task.WhenAll(tasks.ToArray())).ToList();

        if (results.Exists(x => !x) && next != null)
        {
            return await next.HandleAsync(match);
        }

        return results.Exists(x => x);
    }

    private async Task<bool> CheckMarkedTicket(string s, CheckingMarkedTicketCriteria criteria)
    {
        criteria.DbServer = s;
        await repository.ExecuteAsync(criteria);

        return criteria.IsExistMarkedTicket;
    }
}