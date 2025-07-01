using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;

namespace AutoRefundFM;

public class RefundFastMarketWorker(
    IConfiguration configuration,
    IMaintenanceService maintenanceService,
    IMatchPoolService matchPoolService,
    IPushFastMarketResultHandler pushFastMarketResult,
    IUpdateWaterBreakHandler updateWaterBreakHandler) : BackgroundService
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
        var matches = await matchPoolService.PeekNewMatch();
        if (matches.Count == 0)
        {
            return;
        }

        foreach (var item in matches)
        {
            //await updateWaterBreakHandler.Handle(item);
            await pushFastMarketResult.Handle(item);
        }
    }
}
