using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRefundFM;

public static class AutoRefundFMExtensions
{
    public static IServiceCollection AddAutoRefundFM(this IServiceCollection services)
    {
        services
            .AddScoped<IGetFastMarketResultsHandler, GetFastMarketResultsHandler>()
            .AddScoped<IGetSpecialSessionHandler, GetSpecialSessionHandler>()
            .AddScoped<IPushFastMarketResultHandler, PushFastMarketResultHandler>();

        return services;
    }
}
