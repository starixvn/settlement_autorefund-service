using AutoRefund.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.AutoRefund
{
    public static class AutoRefundInjector
    {
        public static void InitializeServices(this IServiceCollection service)
        {
            service.AddTransient<IDynamicRepository, DynamicRepository>();
            service.AddScoped<IMatchPoolService, MatchPoolServiceImp>();
            service.AddScoped<ILogService, LogServiceImp>();
            service.AddScoped<IValidationService, ValidationServiceImp>();
            service.AddScoped<IValidationSettingMatchService, ValidationSettingMatchServiceImp>();
            service.AddScoped<IAutoRefundController, AutoRefundControllerImp>();
            //service.AddScoped<MaintainanceService, MaintainanceServiceImp>();
            service.AddScoped<IRefundMatchService, RefundMatchServiceImp>();
            service.AddScoped<IMatchRefundController, MatchRefundControllerImp>();
            service.AddScoped<IMatchService, MatchServiceImp>();
            service.AddScoped<IValidationTicketMatchService, ValidationTicketServiceImp>();
            service.AddScoped<IValidationMainMatch, ValidationMainMatch>();
        }
    }
}