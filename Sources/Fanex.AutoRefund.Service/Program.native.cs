using AutoRefund;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using ONEbook.UM;
using ONEbook.UM.Configurations;
using SharpRaven;
using System.Collections.Generic;

namespace Fanex.AutoRefund.Service
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StartLogSetting();

            StartDataBaseSetting();

            UMSetting();

            LeagueSettingFactory.SettingFileWatcher();

            ConfigureService.Configure();
        }

        private static void UMSetting()
        {
            var umConfig = new UMClientConfiguration();
            UMClientManager
                .UseConfig(umConfig);
            //.UseRestClient(new RestClient())
            //.UseHttpContext(new HttpContextHelper(app.ApplicationServices.GetService<IHttpContextAccessor>()))
            //.AddErrorHandler(errorHandler);
        }

        private static void StartDataBaseSetting()
        {
            DbSettingProviderManager
              .StartNewSession()
              .Use(AppSettingKeys.ConnectionStrings)
              .WithSqlServer()
              .UseDefaultDbSettingProvider(resourcePath: "~/App_Data")
              .Run();
        }

        private static void StartLogSetting()
        {
            var logEngine = new List<ILogEngine>();

            var sentryEngineOptions = new SentryEngineOptions
            {
                Dsn = new Dsn(AppSettingKeys.Sentrytoken)
            };

            logEngine.Add(new SentryLogging(sentryEngineOptions));

            LogManager.SetDefaultLogCategory(AppSettingKeys.SiteCategory).Use(logEngine);
        }
    }
}