using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Extensions.Hosting;

namespace Fanex.AutoRefund.Service
{
    internal static class ConfigureService
    {
        internal static void Configure()
        {
            var builder = new HostBuilder()
           .ConfigureServices((services) =>
           {
               services.AddHostedService<MainService>();
               services.InitializeServices();
           });

            builder.RunAsTopshelfService(hc =>
            {
                hc.SetServiceName("Fanex.AutoRefund.Service");
                hc.SetDisplayName("Fanex.AutoRefund.Service");
                hc.SetDescription("Auto Refund for the match.");
            });
        }
    }
}