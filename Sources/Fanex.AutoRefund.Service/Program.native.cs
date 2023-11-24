using AutoRefund.Models;
using Fanex.AutoRefund.Service;
using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AutoRefundService = Fanex.AutoRefund.Service.AutoRefund;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.AddFanexSql(builder.Configuration);

builder.Services.AddHostedService<AutoRefundService>();

builder.Services.InitializeServices(builder.Configuration);

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

app.Run();