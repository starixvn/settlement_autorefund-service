using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckMarkedTicket;

public class CheckingMarkedTicketHandler(
    IValidateHandler next,
    IDynamicRepository repository,
    IOptions<AppSettingsKey> appSettingsKey)
    : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
        var criteria = new CheckingMarkedTicketCriteria
        {
            MatchId = match.MatchId,
            ResourceIds = string.Join(",", match.ResultDetails.Select(x => x.ResourceId)),
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckMarkedTicket(s, criteria));

        var results = await Task.WhenAll(tasks.ToArray());

        if (results.ToList().TrueForAll(x => !x) && next != null)
        {
            return await next.HandleAsync(match);
        }

        return new Result
        {
            ErrCode = 111,
            ErrMsg = "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again."
        };
    }

    private async Task<bool> CheckMarkedTicket(string s, CheckingMarkedTicketCriteria criteria)
    {
        criteria.DbServer = s;
        await repository.ExecuteAsync(criteria);

        return criteria.IsExistMarkedTicket;
    }
}