namespace AutoRefund.Services;

using Criteria;
using Exceptions;
using Models;

public interface IValidationTicketMatchService
{
    Task VerifyTicketMatch(Match match);
}

public class ValidationTicketServiceImp(IOptions<AppSettingsKey> appSettingsKey, IDynamicRepository repository)
    : IValidationTicketMatchService
{
    public async Task VerifyTicketMatch(Match match)
    {
        var listTicketInfo = await GetAllTicketInfo(match);

        if (listTicketInfo.Exists(item => item.IsExistMarkedTicket))
        {
            throw new MarkedTicketException();
        }

        if (listTicketInfo.Exists(item => item.IsExistSettledTicket))
        {
            throw new SettledTicketException();
        }
    }

    private async Task<List<MatchTicketInfo>> GetAllTicketInfo(Match match)
    {
        var criteria = new MatchTicketInfoCriteria
        {
            MatchId = match.MatchId,
            TypeId = match.TypeId,
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => GetTicketInfo(s, criteria));
        var listTicketInfo = (await Task.WhenAll(tasks.ToArray())).ToList();
        return listTicketInfo;
    }

    private async Task<MatchTicketInfo> GetTicketInfo(string server, MatchTicketInfoCriteria criteria)
    {
        criteria.DbServer = server;
        return await repository.GetAsync<MatchTicketInfo>(criteria) ?? new MatchTicketInfo();
    }
}