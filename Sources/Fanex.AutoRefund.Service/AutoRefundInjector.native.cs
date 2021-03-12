using AutoRefund.Services;
using Fanex.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;

namespace Fanex.AutoRefund
{
    public static class AutoRefundInjector
    {
        public static void InitializeServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddSingleton<IDynamicRepository, DynamicRepository>();
            service.AddSingleton<IMatchPoolService, MatchPoolServiceImp>();
            service.AddTransient<ILogService, LogServiceImp>();
            service.AddTransient<IValidationService, ValidationServiceImp>();
            service.AddTransient<IAutoRefundController, AutoRefundControllerImp>();
            service.AddTransient<MaintainanceService, MaintainanceServiceImp>();
            service.AddTransient<IValidationSettingMatchService, ValidationSettingMatchServiceImp>();
            service.AddTransient<IValidationTicketMatchService, ValidationTicketServiceImp>();
            service.AddTransient<IRefundMatchService, RefundMatchServiceImp>();
            service.AddTransient<IValidationRelateMatchService, ValidationRelateMatchServiceImp>();
            service.AddTransient<IMatchRefundController, MatchRefundControllerImp>();
            service.AddTransient<IMatchService, MatchServiceImp>();

            service.AddSingleton(new UMClientConfiguration().Initialize(configuration));
            service.AddSingleton<IUMCacheService, UMCacheService>();
            service.AddSingleton<IUMRestClient, UMRestClient>();
            service.AddSingleton<IMaintenanceService, MaintenanceService>();
        }
    }
}