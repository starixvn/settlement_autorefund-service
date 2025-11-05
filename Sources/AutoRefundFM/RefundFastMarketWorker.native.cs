using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Services;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
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
            try
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
            catch (Exception ex)
            {
                var logMessage = new LogMessage(ex.Message)
                    .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                    .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                    .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                    .ToString();
                await LogService.ErrorAsync(logMessage);
            }

        }
    }
}
