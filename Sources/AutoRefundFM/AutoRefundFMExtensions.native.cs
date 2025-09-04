using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRefundFM;

public static class AutoRefundFMExtensions
{
    public static IServiceCollection AddAutoRefundFM(this IServiceCollection services)
    {
        services
            .AddTransient<IGetFastMarketResultsHandler, GetFastMarketResultsHandler>()
            .AddTransient<IGetSpecialSessionHandler, GetSpecialSessionHandler>()
            .AddTransient<IPushFastMarketResultHandler, PushFastMarketResultHandler>()
            .AddTransient<IFastMarketDataTransformationService, FastMarketDataTransformationService>()
            .AddSingleton<IMatchPoolService, MatchPoolServiceImp>()
            .AddSingleton<IUpdateWaterBreakHandler, UpdateWaterBreakHandler>()
            .AddSingleton<ICheckMarkedTicketHandler, CheckMarkedTicketHandler>()
            .AddSingleton<IFmRefundResultService, FmRefundResultService>()
            .AddSingleton<IFastMarketValidationService, FastMarketValidationService>()
            .AddSingleton<IFilterWaterBreakService, FilterWaterBreakService>()
            .AddSingleton<IEventCounterService, EventCounterService>()
            .AddSingleton<SettleOpaqueFacade, SettleOpaqueFacadeImp>();

        return services;
    }
}
