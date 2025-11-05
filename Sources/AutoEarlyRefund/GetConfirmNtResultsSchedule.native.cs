using AutoEarlyRefund.Features.GetConfirmNtResults;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using ONEbook.UM.Services;

namespace AutoEarlyRefund;

public class GetConfirmNtResultsSchedule(
    IConfiguration configuration,
    IMaintenanceService maintenanceService,
    GetConfirmNtResultsHandler confirmNtResultsHandler)
    : BackgroundService
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

                await confirmNtResultsHandler.Handle();
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