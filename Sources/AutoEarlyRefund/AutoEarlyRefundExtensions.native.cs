using AutoEarlyRefund.Features.CheckConditionsRefund;
using AutoEarlyRefund.Features.CheckEventAndTicket;
using AutoEarlyRefund.Features.CheckMarkedTicket;
using AutoEarlyRefund.Features.CheckStatusRefundResults;
using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetConfirmNtResults;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetRefundResults;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Features.RefundConsumerHandler;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Services;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
namespace AutoEarlyRefund;

public static class AutoEarlyRefundExtensions
{
    public static IServiceCollection AddAutoEarlyRefund(this IServiceCollection services)
    {
        services
            .AddSingleton<IValidateHandler, DefaultValidateHandler>()
            .Decorate<IValidateHandler, CheckConditionsRefundHandler>()
            .Decorate<IValidateHandler, CheckEventAndTicketHandler>()
            .Decorate<IValidateHandler, CheckingMarkedTicketHandler>()
            .Decorate<IValidateHandler, CheckStatusRefundResultsHandler>()

            //.Decorate<IValidateHandler, CheckSettleStatusMatchHandler>()
            .AddSingleton<MatchPoolService>()
            .AddSingleton<GetMatchesService, GetMatchesServiceImp>()
            .AddSingleton<IInsOrUpdAutoPayoutResultHandler, InsOrUpdAutoRefundResultHandler>()
            .AddSingleton<IPushResultSettlementHandler, PushResultSettlementHandler>()
            .AddSingleton<IWriteLogService, WriteLogService>()
            .AddSingleton<ISettleOpaqueFacade, SettleOpaqueFacadeImp>()
            .AddSingleton<IGetResourcesTriggerHandler, GetResourcesTriggerHandler>()
            .AddSingleton<IGetResourcesConditionsHandler, GetResourcesConditionsHandler>()
            .AddSingleton<IGetSrcResourcesHandler, GetSrcResourcesHandler>()
            .AddSingleton<IGetRefundMatchInfoHandler, GetRefundMatchInfoHandler>()
            .AddSingleton<CheckConditionsRefundHandler>()
            .AddSingleton<GetResultDetailsHandler>()
            .AddSingleton<GetConfirmNtResultsHandler>()
            .AddSingleton<GetAutoRefundResultsHandler>()
            .AddSingleton<RefundConsumerHandler>()
            .AddMapster();

        return services;
    }
}
