using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Features.CheckMarkedTicket;


public class CheckingMarkedTicketHandler : IValidateHandler
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly IValidateHandler next;
    private readonly ILogger<CheckingMarkedTicketHandler> logger;
    public CheckingMarkedTicketHandler(
        IValidateHandler next,
        IDynamicRepository repository,
        IOptions<AppSettingsKey> appSettingsKey,
        ILogger<CheckingMarkedTicketHandler> logger)
    {
        this.repository = repository;
        this.appSettingsKey = appSettingsKey;
        this.next = next;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(Match match)
    {
        await Fanex.Logging.Logger.Log.WarnAsync($"Before CheckingMarkedTicketHandler: {match.MatchId} {string.Join(",", match.ResultDetails.Select(x => x.ResourceId))} at {DateTime.Now}");

        var criteria = new CheckingMarkedTicketCriteria
        {
            MatchId = match.MatchId,
            ResourceIds = string.Join(",", match.ResultDetails.Select(x => x.ResourceId)),
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckMarkedTicket(s, criteria));

        var results = await Task.WhenAll(tasks.ToArray());


        await Fanex.Logging.Logger.Log.WarnAsync($"After CheckingMarkedTicketHandler: {match.MatchId} {string.Join(",", match.ResultDetails.Select(x => x.ResourceId))} at {DateTime.Now}");

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
