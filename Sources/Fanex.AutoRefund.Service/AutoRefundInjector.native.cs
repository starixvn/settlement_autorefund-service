using AutoMapper;
using AutoRefund.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;

namespace Fanex.AutoRefund;

public static class AutoRefundInjector
{
    public static void InitializeServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<IDynamicRepository, DynamicRepository>();
        service.AddSingleton<IMatchPoolService, MatchPoolServiceImp>();
        service.AddTransient<ILogService, LogServiceImp>();
        service.AddTransient<IValidationService, ValidationServiceImp>();
        service.AddTransient<IAutoRefundController, AutoRefundControllerImp>();
        service.AddTransient<IMaintenanceService, MaintenanceServiceImp>();
        service.AddTransient<IValidationSettingMatchService, ValidationSettingMatchServiceImp>();
        service.AddTransient<IValidationTicketMatchService, ValidationTicketServiceImp>();
        service.AddTransient<IRefundMatchService, RefundMatchServiceImp>();
        service.AddTransient<IValidationRelateMatchService, ValidationRelateMatchServiceImp>();
        service.AddTransient<IMatchRefundController, MatchRefundControllerImp>();
        service.AddTransient<IMatchService, MatchServiceImp>();

        service.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        service.AddSingleton<IUMCacheService, UMCacheService>();
        service.AddSingleton<IUMRestClient, UMRestClient>();
        service.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddSingleton<ScaleOutSettleService, ScaleOutSettleServiceImp>();

        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(typeof(ScaleOutMappingProfile));
        });

        var mapper = mapperConfig.CreateMapper();
        service.AddSingleton(mapper);
        ConfigureRedis(service, configuration);
    }

    private static void ConfigureRedis(IServiceCollection service, IConfiguration configuration)
    {
        var redisHost = configuration.GetSection("Redis.Server").Value;
        var redisPort = configuration.GetSection("Redis.Port").Value;
        var redisPassword = configuration.GetSection("Redis.Password").Value;
        var redisDatabase = configuration.GetSection("Redis.DefaultDatabase").Value;
        service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = $"{redisHost}:{redisPort},password={redisPassword},defaultDatabase={redisDatabase}",
            ApplicationName = configuration.GetSection("Redis.ApplicationName").Value,
        }));
        service.AddSingleton<ICacheService>(new CacheService());
    }
}