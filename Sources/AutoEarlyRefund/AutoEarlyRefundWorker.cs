using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using ONEbook.UM.Services;

namespace AutoEarlyRefund;

public class AutoEarlyRefundWorker(
    IConfiguration configuration,
    MatchPoolService matchPoolService,
    IValidateHandler validateHandler,
    ISettleOpaqueFacade settleOpaqueFacade,
    IMaintenanceService maintenanceService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //var intervalTime = TimeSpan.FromMilliseconds(configuration.GetValue<int>("ServiceInterval"));
        //using PeriodicTimer timer = new(intervalTime);

        ////while (await timer.WaitForNextTickAsync(stoppingToken))
        //{
        //    var isMaintenance = await maintenanceService.IsUnderMaintenanceAsync();
        //    if (isMaintenance)
        //    {
        //        return;
        //    }

        //    var matches = await matchPoolService.PeekNewMatch();
        //    matches.ForEach(match =>
        //    {
        //        Task.Run(() => settleOpaqueFacade.Settle(match));
        //    });
        //}

        List<Match> matches = [
            new Match
        {
            MatchId = 1,
            ResultDetails = [
                new ResultDetail
                {
                    ResourceId = "01",
                    HomeValue = "1",
                    AwayValue = "0"
                }
                ]
        }];

        matches.ForEach(match =>
        {
            Task.Run(() => validateHandler.HandleAsync(match));
        });
    }
}
