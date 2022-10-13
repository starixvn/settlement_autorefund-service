namespace AutoRefund.Services;

public interface IAutoRefundController
{
    Task ProcessAsync();
}

public class AutoRefundControllerImp : IAutoRefundController
{
    private readonly IMatchPoolService matchPoolService;
    private readonly IMatchRefundController matchRefundController;

    public AutoRefundControllerImp(
        IMatchPoolService matchPoolService,
        IMatchRefundController matchRefundController)
    {
        this.matchPoolService = matchPoolService;
        this.matchRefundController = matchRefundController;
    }

    public async Task ProcessAsync()
    {
        var newMatches = (await matchPoolService.PeekNewMatch()).Where(n => !n.IsChecking).ToList();
        foreach (var match in newMatches)
        {
            await matchRefundController.Process(match).ConfigureAwait(false);
        }
    }
}