using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;

namespace AutoRefundFM;

public class RefundFastMarketWorker(
    IConfiguration configuration,
    IMaintenanceService maintenanceService,
    IMatchPoolService matchPoolService,
    SettleOpaqueFacade settleOpaqueFacade) : BackgroundService
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
                continue;
            }

            var matches = await matchPoolService.PeekNewMatch();

            if (matches == null || matches.Count == 0)
            {
                continue;
            }

            foreach (var item in matches)
            {
                await settleOpaqueFacade.Refund(item);
            }

        }
    }
}
