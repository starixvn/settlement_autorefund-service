using AutoEarlyRefund.Consumers;
using AutoMapper;
using AutoRefund.Core.Services;
using AutoRefund.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut;
using Fanex.Settlement.ScaleOut.Services;
using MassTransit;
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
        service.AddCoreService();
    }

    public static void AddCoreService(this IServiceCollection services)
    {
        services.AddSingleton<AutoRefundResultService, AutoRefundResultServiceImp>();
        services.AddSingleton<SettlementLogService, SettlementLogServiceImp>();
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

    public static void AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<ScaleOutMappingProfile>());
    }

    public static void AddMassTransit(this IServiceCollection service, IConfiguration configuration)
    {
        var host = configuration.GetValue("RabbitMQ:Host", string.Empty);
        var virtualHost = configuration.GetValue("RabbitMQ:VirtualHost", string.Empty);
        var username = configuration.GetValue("RabbitMQ:Username", string.Empty);
        var password = configuration.GetValue("RabbitMQ:Password", string.Empty);

        service.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<RefundConsumer>();

            x.UsingRabbitMq((context, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.UseMessageRetry(r =>
                {
                    r.Handle<RabbitMqConnectionException>();
                    r.Interval(5, TimeSpan.FromSeconds(10));
                });

                busFactoryConfigurator.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                busFactoryConfigurator.ConfigureEndpoints(context);
            });
        });
    }
}