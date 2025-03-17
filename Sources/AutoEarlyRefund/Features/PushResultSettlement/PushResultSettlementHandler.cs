using AutoEarlyRefund.Models;
using AutoRefund.Core.Models;
using Microsoft.Extensions.Options;

namespace AutoEarlyRefund.Features.PushResultSettlement;

public interface IPushResultSettlementHandler
{
    Task HandleAsync(Match match, CancellationToken cancellationToken);
}

public class PushResultSettlementHandler : IPushResultSettlementHandler
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;

    public PushResultSettlementHandler(IDynamicRepository repository, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.appSettingsKey = appSettingsKey ?? throw new ArgumentNullException(nameof(appSettingsKey));
    }

    public async Task HandleAsync(Match match, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(match);

        var criteria = new PushResultSettlementCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            IsAuto = true,
            SelectedResources = match.ResourcesXml,
        };

        var servers = appSettingsKey.Value.SiteDbServer ?? new List<string>();
        var tasks = servers.Select(s => PushResultSettlement(s, criteria, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task PushResultSettlement(string server, PushResultSettlementCriteria criteria, CancellationToken cancellationToken)
    {
        criteria.DbServer = server;
        await repository.ExecuteAsync(criteria, "", cancellationToken);
    }
}