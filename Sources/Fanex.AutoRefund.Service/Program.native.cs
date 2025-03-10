using AutoEarlyRefund;
using AutoEarlyRefund.Consumers;
using AutoRefund.Core.Models;
using Fanex.AutoRefund.Service;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;
//using AutoRefundService = Fanex.AutoRefund.Service.AutoRefund;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<RefundConsumer>();


    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });

        cfg.Host(builder.Configuration["RabbitMQ:Host"], builder.Configuration["RabbitMQ:VirtualHost"], h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMQ:Username", "guest"));
            h.Password(builder.Configuration.GetValue("RabbitMQ:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });

});

builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.AddOneBookUm(builder.Configuration);

builder.Services.AddFanexLogSentry(builder.Configuration);

builder.Services.AddFanexSql(builder.Configuration);

builder.Services.AddConfigRedisCache(builder.Configuration);

builder.Services.AddAutoMapper();

builder.Services.InitializeServices(builder.Configuration);

builder.Services.AddAutoEarlyRefund();

//builder.Services.AddHostedService<AutoRefundService>();
//builder.Services.AddHostedService<AutoEarlyRefundWorker>();
builder.Services.AddHostedService<GetConfirmNtResultsSchedule>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();