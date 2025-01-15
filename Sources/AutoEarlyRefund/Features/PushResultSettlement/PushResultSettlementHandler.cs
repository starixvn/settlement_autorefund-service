using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.PushResultSettlement;

public interface IPushResultSettlementHandler
{
    Task HandleAsync(Match match, CancellationToken cancellationToken);
}

public class PushResultSettlementHandler(IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey)
    : IPushResultSettlementHandler
{
    public async Task HandleAsync(Match match, CancellationToken cancellationToken)
    {
        var criteria = new PushResultSettlementCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            IsAuto = true,
            SelectedResources = match.ResourcesXml,
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => PushResultSettlement(s, criteria, cancellationToken));
        await Task.WhenAll(tasks.ToArray());
    }

    private async Task PushResultSettlement(string s, PushResultSettlementCriteria criteria, CancellationToken cancellationToken)
    {
        criteria.DbServer = s;
        await repository.ExecuteAsync(criteria, cancellationToken: cancellationToken);
    }
}