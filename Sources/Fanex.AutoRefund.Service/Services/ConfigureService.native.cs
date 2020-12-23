using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Extensions.Hosting;

namespace Fanex.AutoRefund.Service
{
    internal static class ConfigureService
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
       Host.CreateDefaultBuilder(args)
           .ConfigureServices(services =>
           {
               services.AddHostedService<MainService>();
               services.InitializeServices();
           }).UseSystemd();

    }
}