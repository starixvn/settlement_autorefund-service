using AutoRefund;
using AutoRefund.Services;
using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using SharpRaven;
using LogService = Fanex.Logging.LogService;

namespace Fanex.AutoRefund.Service;

public class AutoRefund : IHostedService
{
    private readonly IMaintenanceService maintenanceService;

    private readonly IAutoRefundController autoRefundController;

    private readonly IMatchService matchService;

    private readonly IConfiguration configuration;

    private bool running;

    private int counter;

    private int timeCounter;

    public AutoRefund(
        IAutoRefundController autoRefundController,
        IConfiguration configuration,
        IMatchService matchService,
        IMaintenanceService maintenanceService)
    {
        this.autoRefundController = autoRefundController;
        this.matchService = matchService;
        this.maintenanceService = maintenanceService;
        this.configuration = configuration;
        InitializeExtraComponent();
        InitializeSetting();
    }

    private void InitializeSetting()
    {
        StartUmSetting();
        StartDataBaseSetting();
        StartLogSetting();
        LeagueSettingFactory.SettingFileWatcher();
    }

    private static void InitializeExtraComponent()
    {
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
    }

    private static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        e.Exception.Handle(x =>
        {
            LogService.Error(x.Message, x);
            return true;
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        running = true;
        var autoRefundThread = new Thread(RefundThreadProcess);
        Console.WriteLine($"AutoRefund Starting at {DateTime.Now}");
        autoRefundThread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        Console.WriteLine($"AutoRefund Stopping at {DateTime.Now}");
        return Task.CompletedTask;
    }

    private void RefundThreadProcess()
    {
        while (running)
        {
            if (!maintenanceService.IsMaintenance)
            {
                MainProcess();
                Thread.Sleep(int.Parse(configuration.GetSection("ServiceInterval").Value));
            }
        }
    }

    private void MainProcess()
    {
        try
        {
            autoRefundController.ProcessAsync();
            HandleMissedMatch();
            HandleGarbageCollector();
        }
        catch (Exception exception)
        {
            LogService.Error("Fanex.AutoRefund.Service:", exception);
        }
    }

    private void HandleGarbageCollector()
    {
        if (counter >= int.Parse(configuration.GetSection("GarbageCollectorInterval").Value))
        {
            counter = 0;
            GC.WaitForPendingFinalizers();
        }

        counter += int.Parse(configuration.GetSection("ServiceInterval").Value);
    }

    private void HandleMissedMatch()
    {
        if (timeCounter >= int.Parse(configuration.GetSection("CheckMissedMatchInterval").Value))
        {
            timeCounter = 0;
            matchService.CheckMissedMatch();
        }

        timeCounter += int.Parse(configuration.GetSection("ServiceInterval").Value);
    }

    private void StartUmSetting()
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
            Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
        };

        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory(configuration.GetSection("SiteCategory").Value).Use(logEngine);
    }
}