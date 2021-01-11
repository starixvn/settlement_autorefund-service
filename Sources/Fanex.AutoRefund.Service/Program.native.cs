using AutoRefund.Models;
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
           .ConfigureServices((hostContext, services) =>
           {
               services
               .Configure<AppSettingsKey>(hostContext.Configuration)
               .AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettingsKey>>().Value);
               services.AddHostedService<Startup>();
               services.InitializeServices();
           })
            .UseSystemd();

    }
}