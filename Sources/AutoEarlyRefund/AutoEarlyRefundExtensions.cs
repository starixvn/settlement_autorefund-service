using AutoEarlyRefund.Features.CheckEventAndTicket;
using AutoEarlyRefund.Features.CheckMarkedTicket;
using AutoEarlyRefund.Features.CheckSettleStatusMatch;
using AutoEarlyRefund.Features.FetchItems;
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
        services.AddSingleton<IValidateHandler, CheckingMarkedTicketHandler>()
            .Decorate<IValidateHandler, CheckSettleStatusMatchHandler>()
            .Decorate<IValidateHandler, CheckEventAndTicketHandler>()

            .AddSingleton<MatchPoolService>()
            .AddSingleton<GetMatchesService, GetMatchesServiceImp>()
            .AddSingleton<IInsOrUpdAutoPayoutResultHandler, InsOrUpdAutoPayoutResultHandler>()
            .AddSingleton<IPushResultSettlementHandler, PushResultSettlementHandler>()
            .AddSingleton<IWriteLogService, WriteLogService>()
            .AddSingleton<ISettleOpaqueFacade, SettleOpaqueFacadeImp>();

        return services;
    }
}
