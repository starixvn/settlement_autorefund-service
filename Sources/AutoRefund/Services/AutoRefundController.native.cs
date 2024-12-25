namespace AutoRefund.Services;

public interface IAutoRefundController
{
    Task ProcessAsync();
}

public class AutoRefundControllerImp(
    IMatchPoolService matchPoolService,
    IMatchRefundController matchRefundController)
    : IAutoRefundController
{
    public async Task ProcessAsync()
    {
        var newMatches = (await matchPoolService.PeekNewMatch()).Where(n => !n.IsChecking).ToList();

        newMatches.ForEach(m =>
        {
            Task.Run(() => matchRefundController.Process(m))
                .ConfigureAwait(false);
        });
    }
}