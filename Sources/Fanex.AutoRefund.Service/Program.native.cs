using AutoEarlyRefund;
using AutoRefund.Core.Models;
using AutoRefundFM;
using Fanex.AutoRefund.Service;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.Configure<AppSettingsKey>(configuration);

services.AddMassTransit(configuration);

services.AddOneBookUmForService(configuration);

services.AddFanexLogSentry(configuration);

services.AddFanexSql(configuration);

services.AddConfigRedisCache(configuration);

services.InitializeServices(configuration);

services.AddAutoMapper();

services.AddAutoEarlyRefund();

builder.Services.AddAutoRefundFM();

//services.AddHostedService<AutoRefundService>();
//services.AddHostedService<AutoEarlyRefundWorker>();
//services.AddHostedService<GetConfirmNtResultsSchedule>();
builder.Services.AddHostedService<ReufundFastMarketWorker>();
services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/healthz");

await app.RunAsync();