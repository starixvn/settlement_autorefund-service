using AutoEarlyRefund.Features.CheckEventAndTicket;
using AutoEarlyRefund.Features.CheckSettleStatusMatch;
using AutoEarlyRefund.Features.FetchItems;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoEarlyRefund;

public static class AutoEarlyRefundExtensions
{
    public static IServiceCollection AddAutoEarlyRefund(this IServiceCollection services)
    {
        services
            .AddSingleton<IValidateHandler, DefaultValidateHandler>()
            .Decorate<IValidateHandler, CheckSettleStatusMatchHandler>()
            .Decorate<IValidateHandler, CheckEventAndTicketHandler>()

            .AddSingleton<MatchPoolService>()
            .AddSingleton<GetMatchesService, GetMatchesServiceImp>()
            .AddSingleton<IInsOrUpdAutoPayoutResultHandler, InsOrUpdAutoRefundResultHandler>()
            .AddSingleton<IPushResultSettlementHandler, PushResultSettlementHandler>()
            .AddSingleton<IWriteLogService, WriteLogService>()
            .AddSingleton<ISettleOpaqueFacade, SettleOpaqueFacadeImp>()
            .AddSingleton<GetResourcesTriggerHandler>()
            .AddSingleton<GetResourcesConditionsHandler>();

        return services;
    }
}
