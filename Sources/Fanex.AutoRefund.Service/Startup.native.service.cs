using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Microsoft.Extensions.Configuration;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;
using Sentry;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.AutoRefund.Service
{
    public partial class Startup
    {
        private void StartUMSetting()
        {
            var umConfig = new UMClientConfiguration().Initialize(configuration);

            UMClientManager
               .UseConfig(umConfig)
               .UseCacheService(new UMCacheService(new CacheService()))
               .UseRestClient(new UMRestClient(umConfig));
        }

        private void StartDataBaseSetting()
        {
            var connections = configuration.GetSection("ConnectionStrings")
                                   .GetChildren()
                                   .AsEnumerable()
                                   .ToDictionary(
                                            x => x.GetValue<string>("Name"),
                                            y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
                                    );
            DbSettingProviderManager
              .StartNewSession()
              .Use(connections)
              .WithSqlServer()
              .UseDefaultDbSettingProvider(resourcePath: configuration.GetSection("StoredProcedurePath").Value)
              .Run();
        }

        private void StartLogSetting()
        {
            var logEngine = new List<ILogEngine>();

            var sentryEngineOptions = new SentryEngineOptions
            {
                Dsn = new Dsn(configuration.GetSection("Sentrytoken").Value)
            };

            logEngine.Add(new SentryLogging(sentryEngineOptions));

            LogManager.SetDefaultLogCategory(configuration.GetSection("SiteCategory").Value).Use(logEngine);
        }
    }
}