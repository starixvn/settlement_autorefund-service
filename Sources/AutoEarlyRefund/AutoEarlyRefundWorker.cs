using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Services;
using ONEbook.UM.Services;

namespace AutoEarlyRefund;

public class AutoEarlyRefundWorker(
    IConfiguration configuration,
    MatchPoolService matchPoolService,
    ISettleOpaqueFacade settleOpaqueFacade,
    IMaintenanceService maintenanceService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalTime = TimeSpan.FromMilliseconds(configuration.GetValue<int>("ServiceInterval"));
        using PeriodicTimer timer = new(intervalTime);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var isMaintenance = await maintenanceService.IsUnderMaintenanceAsync();
            if (isMaintenance)
            {
                return;
            }

            var matches = await matchPoolService.PeekNewMatch();

            matches.ForEach(x => Task.Run(() => settleOpaqueFacade.Settle(x)));

            //foreach (var item in matches)
            //{
            //    //await settleOpaqueFacade.Settle(item);
            //}
        }
    }
}
