using AutoEarlyRefund.Features.GetConfirmNtResults;
using ONEbook.UM.Services;

namespace AutoEarlyRefund;

public class GetConfirmNtResultsSchedule : BackgroundService
{
    private readonly IConfiguration configuration;
    private readonly IMaintenanceService maintenanceService;
    private readonly GetConfirmNtResultsHandler confirmNtResultsHandler;

    public GetConfirmNtResultsSchedule(IConfiguration configuration, IMaintenanceService maintenanceService, GetConfirmNtResultsHandler confirmNtResultsHandler)
    {
        this.configuration = configuration;
        this.maintenanceService = maintenanceService;
        this.confirmNtResultsHandler = confirmNtResultsHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalTime = TimeSpan.FromMilliseconds(configuration.GetValue<int>("ServiceInterval"));
        using PeriodicTimer timer = new(intervalTime);

        //while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var isMaintenance = await maintenanceService.IsUnderMaintenanceAsync();
            if (isMaintenance)
            {
                return;
            }

            await confirmNtResultsHandler.Handle();
        }
    }
}
