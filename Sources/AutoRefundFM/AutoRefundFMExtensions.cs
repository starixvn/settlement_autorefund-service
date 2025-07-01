using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.InsOrUpdAutoRefundResult;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
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
            .AddSingleton<IInsOrUpdAutoPayoutResultHandler, InsOrUpdAutoRefundResultHandler>();

        return services;
    }
}
