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
            service.AddTransient<IDynamicRepository, DynamicRepository>();
            service.AddScoped<IMatchPoolService, MatchPoolServiceImp>();
            service.AddScoped<ILogService, LogServiceImp>();
            service.AddScoped<IValidationService, ValidationServiceImp>();
            service.AddScoped<IAutoRefundController, AutoRefundControllerImp>();
            service.AddScoped<MaintainanceService, MaintainanceServiceImp>();
            service.AddScoped<IRefundMatchService, RefundMatchServiceImp>();
            service.AddScoped<IMatchRefundController, MatchRefundControllerImp>();
            service.AddScoped<IMatchService, MatchServiceImp>();
        }
    }
}