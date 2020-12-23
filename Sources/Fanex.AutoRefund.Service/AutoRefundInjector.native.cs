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
            service.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            service.AddTransient<IDynamicRepository, DynamicRepository>();
            service.AddTransient<IMatchPoolService, MatchPoolServiceImp>();
            service.AddTransient<ILogService, LogServiceImp>();
            service.AddTransient<IValidationService, ValidationServiceImp>();
            service.AddTransient<IAutoRefundController, AutoRefundControllerImp>();
           // service.AddTransient<MaintainanceService, MaintainanceServiceImp>();
            service.AddTransient<IRefundMatchService, RefundMatchServiceImp>();
            service.AddTransient<IMatchRefundController, MatchRefundControllerImp>();
            service.AddTransient<IMatchService, MatchServiceImp>();
        }
    }
}