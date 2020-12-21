using AutoRefund;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
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

            LeagueSettingFactory.SettingFileWatcher();

            ConfigureService.Configure();
        }

        private static void StartDataBaseSetting()
        {
            DbSettingProviderManager
              .StartNewSession()
              .Use(AppSettingKeys.ConnectionStrings)
              .WithSqlServer()
              .UseDefaultDbSettingProvider(AppSettingKeys.StoredProcedurePath)
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