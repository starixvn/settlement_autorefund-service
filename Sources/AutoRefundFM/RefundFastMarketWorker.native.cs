using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;

namespace AutoRefundFM;

public class RefundFastMarketWorker(
    IConfiguration configuration,
    IMaintenanceService maintenanceService,
    ISessionPoolService matchPoolService,
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

            var sessions = await matchPoolService.PeekSessions();

            sessions.ForEach(session =>
            {
                Task.Run(() => settleOpaqueFacade.Refund(session));
            });
        }
    }
}
