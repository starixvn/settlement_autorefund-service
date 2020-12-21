using Fanex.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.AutoRefund.Service
{
    public static class AppSettingKeys
    {
        static AppSettingKeys()
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

            StoredProcedurePath = configuration["StoredProcedurePath"];
            SiteCategory = configuration["SiteCategory"];
            Sentrytoken = configuration["Sentrytoken"];
            RetryCount = int.Parse(configuration["RetryCount"]);
            RetryInterval = int.Parse(configuration["RetryInterval"]);
            ServiceInterval = int.Parse(configuration["ServiceInterval"]);
            GarbageCollectorInterval = int.Parse(configuration["GarbageCollectorInterval"]);
            LeagueSettingPath = configuration["LeagueSettingPath"];
            LeagueSettingFile = configuration["LeagueSettingFile"];

            ConnectionStrings = configuration.GetSection("ConnectionStrings").GetChildren().AsEnumerable().ToDictionary(
                key => key.GetValue<string>("Name"),
                value => new ConnectionConfiguration(value.GetValue<string>("Name"), value.GetValue<string>("ConnectionString"), value.GetValue<string>("ProviderName")));
        }

        public static Dictionary<string, ConnectionConfiguration> ConnectionStrings { get; private set; }

        public static string StoredProcedurePath { get; private set; }

        public static string SiteCategory { get; private set; }

        public static string Sentrytoken { get; private set; }

        public static int RetryCount { get; private set; }

        public static int RetryInterval { get; private set; }

        public static int ServiceInterval { get; private set; }

        public static int GarbageCollectorInterval { get; private set; }

        public static string LeagueSettingPath { get; private set; }

        public static string LeagueSettingFile { get; private set; }
    }
}