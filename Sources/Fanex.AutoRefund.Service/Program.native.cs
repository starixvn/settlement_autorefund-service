using AutoRefund.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fanex.AutoRefund.Service
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://0.0.0.0:5000");
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .Configure<AppSettingsKey>(hostContext.Configuration)
                        .AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettingsKey>>().Value);
                    services.AddHostedService<AutoRefund>();
                    services.InitializeServices(hostContext.Configuration);
                });
    }
}