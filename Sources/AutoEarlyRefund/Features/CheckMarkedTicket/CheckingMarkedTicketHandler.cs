namespace AutoEarlyRefund.Features.CheckMarkedTicket;

public class CheckingMarkedTicketHandler
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;

    public CheckingMarkedTicketHandler(IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.appSettingsKey = appSettingsKey;
    }

    public async Task<bool> HandleAsync(int matchId, CancellationToken cancellationToken)
    {
        var criteria = new CheckingMarkedTicketCriteria
        {
            MatchId = matchId
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckMarkedTicket(s, criteria, cancellationToken));

        var results = await Task.WhenAll(tasks.ToArray());

        return results.Any(x => x);
    }

    private async Task<bool> CheckMarkedTicket(string s, CheckingMarkedTicketCriteria criteria, CancellationToken cancellationToken)
    {
        criteria.DbServer = s;
        await repository.ExecuteAsync(criteria, cancellationToken: cancellationToken);

        return criteria.IsExistMarkedTicket;
    }
}
