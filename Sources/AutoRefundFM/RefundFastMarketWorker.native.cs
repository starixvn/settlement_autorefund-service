using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;

namespace AutoRefundFM;

public class RefundFastMarketWorker(
    IConfiguration configuration,
    IMaintenanceService maintenanceService,
    IGetFastMarketResultsHandler getFastMarketResults,
    IGetSpecialSessionHandler getSpecialSessionHandler,
    IPushFastMarketResultHandler pushFastMarketResult) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //var intervalTime = TimeSpan.FromMilliseconds(configuration.GetValue<int>("ServiceInterval"));
        //using PeriodicTimer timer = new(intervalTime);

        //while (await timer.WaitForNextTickAsync(stoppingToken))
        //{
        //    var isMaintenance = await maintenanceService.IsUnderMaintenanceAsync();
        //    if (isMaintenance)
        //    {
        //        continue;
        //    }

        //}
        var matches = await getSpecialSessionHandler.Handle();
        if (matches.Count == 0)
        {
            return;
        }

        foreach (var item in matches)
        {
            await pushFastMarketResult.Handle(item);
        }
    }
}
