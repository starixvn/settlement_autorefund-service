using AutoRefund.Models;
using Fanex.AutoRefund.Service;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;
using AutoRefundService = Fanex.AutoRefund.Service.AutoRefund;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.AddOneBookUm(builder.Configuration);

builder.Services.AddFanexLogSentry(builder.Configuration);

builder.Services.AddFanexSql(builder.Configuration);

builder.Services.AddConfigRedisCache(builder.Configuration);

builder.Services.AddAutoMapper();

builder.Services.AddHostedService<AutoRefundService>();

builder.Services.InitializeServices(builder.Configuration);

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();