using AutoMapper;
using AutoRefund.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.AutoRefund.Service;

public static class AutoRefundInjector
{
    public static void InitializeServices(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<IDynamicRepository, DynamicRepository>();
        service.AddSingleton<IMatchPoolService, MatchPoolServiceImp>();
        service.AddTransient<LogService, LogServiceImp>();
        service.AddTransient<IValidationService, ValidationServiceImp>();
        service.AddTransient<IAutoRefundController, AutoRefundControllerImp>();
        service.AddTransient<IValidationSettingMatchService, ValidationSettingMatchServiceImp>();
        service.AddTransient<IValidationTicketMatchService, ValidationTicketServiceImp>();
        service.AddTransient<IRefundMatchService, RefundMatchServiceImp>();
        service.AddTransient<IValidationRelateMatchService, ValidationRelateMatchServiceImp>();
        service.AddTransient<IMatchRefundController, MatchRefundControllerImp>();
        service.AddTransient<IMatchService, MatchServiceImp>();
        service.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        service.AddSingleton<ScaleOutSettleService, ScaleOutSettleServiceImp>();
        service.AddTransient<GeneralService, GeneralServiceImp>();
        service.AddTransient<XmlConvertService, XmlConvertServiceImp>();
    }

    public static void AddConfigRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Redis:ConnectionString");
        var applicationName = configuration.GetValue<string>("Redis:ApplicationName");

        services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = connectionString,
            ApplicationName = applicationName,
        }));
        services.AddSingleton<ICacheService>(new CacheService());
    }

    public static void AddAutoMapper(this IServiceCollection service)
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(typeof(ScaleOutMappingProfile));
        });

        var mapper = mapperConfig.CreateMapper();
        service.AddSingleton(mapper);
    }
}