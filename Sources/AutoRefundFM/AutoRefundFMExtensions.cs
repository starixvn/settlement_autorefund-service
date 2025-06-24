using AutoRefundFM.Features.GetFastMarketResults;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRefundFM;

public static class AutoRefundFMExtensions
{
    public static IServiceCollection AddAutoRefundFM(this IServiceCollection services)
    {
        services
            .AddScoped<GetFastMarketResultsHandler>();

        return services;
    }
}
