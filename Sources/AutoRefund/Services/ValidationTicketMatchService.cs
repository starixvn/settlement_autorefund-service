using AutoRefund.Criteria;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;

namespace AutoRefund.Services;

public interface IValidationTicketMatchService
{
    Task VerifyTicketMatch(Match match);
}

public class ValidationTicketServiceImp : IValidationTicketMatchService
{
    private readonly AppSettingsKey appSettingsKey;

    private readonly IDynamicRepository repository;

    public ValidationTicketServiceImp(IOptions<AppSettingsKey> appSettingsKey, IDynamicRepository repository)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.repository = repository;
    }

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
        var listTicketInfo = new List<MatchTicketInfo>();
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            var criteria = new MatchTicketInfoCriteria
            {
                MatchId = match.MatchId,
                TypeId = match.TypeId,
                DbServer = server
            };

            var ticketInfo = await repository.GetAsync<MatchTicketInfo>(criteria) ?? new MatchTicketInfo();
            listTicketInfo.Add(ticketInfo);
        }

        return listTicketInfo;
    }
}