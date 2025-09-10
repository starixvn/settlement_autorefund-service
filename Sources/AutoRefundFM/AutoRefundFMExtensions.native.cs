using AutoRefundFM.Features.CheckMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Services;
using AutoRefundFM.Services.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace AutoRefundFM;

public static class AutoRefundFMExtensions
{
    public static IServiceCollection AddAutoRefundFM(this IServiceCollection services)
    {
        services
            .AddSingleton<IValidationHandler, DefaultValidationHandler>()

            .Decorate<IValidationHandler, EventTicketValidationHandler>()
            .Decorate<IValidationHandler, MarkedTicketsValidationHandler>()

            .Decorate<IValidationHandler, CompletedResultsValidationHandler>()
            .Decorate<IValidationHandler, ExistingRefundsValidationHandler>()

            .Decorate<IValidationHandler, EventCounterValidationHandler>()
            .Decorate<IValidationHandler, ChoiceResultValidationHandler>();

        services
            .AddTransient<IGetFastMarketResultsHandler, GetFastMarketResultsHandler>()
            .AddTransient<IGetSpecialSessionHandler, GetSpecialSessionHandler>()
            .AddTransient<IPushFastMarketResultHandler, PushFastMarketResultHandler>()
            .AddTransient<IDataTransformationService, DataTransformationService>()
            .AddSingleton<ISessionPoolService, SessionPoolService>()
            .AddSingleton<IUpdateWaterBreakHandler, UpdateWaterBreakHandler>()
            .AddSingleton<ICheckMarkedTicketHandler, CheckMarkedTicketHandler>()
            .AddSingleton<IFmRefundResultService, FmRefundResultService>()
            .AddSingleton<IFilterWaterBreakService, FilterWaterBreakService>()
            .AddSingleton<IEventCounterService, EventCounterService>()
            .AddSingleton<SettleOpaqueFacade, SettleOpaqueFacadeImp>();

        return services;
    }
}
