namespace AutoEarlyRefund.Features.CheckSettleStatusMatch;

public class CheckSettleStatusMatchHandler
{
    private readonly IDynamicRepository repository;
    public CheckSettleStatusMatchHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task HandleAsync(int matchId, CancellationToken cancellationToken)
    {
        var criteria = new CheckSettleStatusMatchCriteria
        {
            MatchId = matchId
        };
        await repository.ExecuteAsync(criteria, cancellationToken: cancellationToken);
    }
}
