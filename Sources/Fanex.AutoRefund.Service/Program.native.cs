using AutoEarlyRefund;
using AutoRefund.Core.Models;
using Fanex.AutoRefund.Service;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<AppSettingsKey>(configuration);

builder.Services.AddMassTransit(configuration);

builder.Services.AddOneBookUmForService(configuration);

builder.Services.AddFanexLogSentry(configuration);

builder.Services.AddFanexSql(configuration);

builder.Services.AddConfigRedisCache(configuration);

builder.Services.InitializeServices(configuration);

builder.Services.AddAutoMapper();

builder.Services.AddAutoEarlyRefund();

builder.Services.AddHostedService<AutoRefundService>();
builder.Services.AddHostedService<AutoEarlyRefundWorker>();
builder.Services.AddHostedService<GetConfirmNtResultsSchedule>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();