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
            .AddSingleton<IMatchPoolService, MatchPoolServiceImp>()
            .AddSingleton<IUpdateWaterBreakHandler, UpdateWaterBreakHandler>()
            .AddSingleton<ICheckMarkedTicketHandler, CheckMarkedTicketHandler>()
            .AddSingleton<IFmRefundResultService, FmRefundResultService>()
            .AddSingleton<IValidateSessionService, ValidateSessionService>()
            .AddSingleton<SettleOpaqueFacade, SettleOpaqueFacadeImp>();

        return services;
    }
}
