using AutoEarlyRefund.Features.GetConfirmNtResults;
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
            var isMaintenance = await maintenanceService.IsUnderMaintenanceAsync();
            if (isMaintenance) //
            {
                continue;
            }

            await confirmNtResultsHandler.Handle();
        }
    }
}