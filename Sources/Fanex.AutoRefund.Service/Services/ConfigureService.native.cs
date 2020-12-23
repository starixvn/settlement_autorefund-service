using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Extensions.Hosting;

namespace Fanex.AutoRefund.Service
{
    internal static class ConfigureService
    {
        //internal static void Configure()
        //{
        //   // var builder = new HostBuilder()
        //   // .ConfigureAppConfiguration((hostingContext, config) =>
        //   // {
        //   //     config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //   //     config.AddEnvironmentVariables();
        //   // })
        //   //.ConfigureServices((services) =>
        //   //{
        //   //    services.AddHostedService<MainService>();
        //   //    services.InitializeServices();
        //   //}).;

        //   // builder.RunAsTopshelfService(hc =>
        //   // {
        //   //     hc.SetServiceName("Fanex.AutoRefund.Service");
        //   //     hc.SetDisplayName("Fanex.AutoRefund.Service");
        //   //     hc.SetDescription("Auto Refund for the match.");
        //   // });


        //}
        public static IHostBuilder CreateHostBuilder(string[] args) =>
       Host.CreateDefaultBuilder(args)
            .UseSystemd()
           .ConfigureServices(services =>
           {
               services.AddHostedService<MainService>();
               services.InitializeServices();
           });

    }
}