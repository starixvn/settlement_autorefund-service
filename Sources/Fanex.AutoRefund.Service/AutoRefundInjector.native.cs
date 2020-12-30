using AutoRefund.Services;
using Fanex.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.AutoRefund
{
    public static class AutoRefundInjector
    {
        public static void InitializeServices(this IServiceCollection service)
        {
            service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            service.AddSingleton<IDynamicRepository, DynamicRepository>();
            service.AddSingleton<IMatchPoolService, MatchPoolServiceImp>();
            service.AddTransient<ILogService, LogServiceImp>();
            service.AddTransient<IValidationService, ValidationServiceImp>();
            service.AddTransient<IAutoRefundController, AutoRefundControllerImp>();
            // service.AddTransient<MaintainanceService, MaintainanceServiceImp>();
            service.AddTransient<IValidationSettingMatchService, ValidationSettingMatchServiceImp>();
            service.AddTransient<IValidationTicketMatchService, ValidationTicketServiceImp>();
            service.AddTransient<IRefundMatchService, RefundMatchServiceImp>();
            service.AddTransient<IValidationRelateMatchService, ValidationRelateMatchServiceImp>();
            service.AddTransient<IMatchRefundController, MatchRefundControllerImp>();
            service.AddTransient<IMatchService, MatchServiceImp>();
        }
    }
}