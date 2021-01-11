using AutoRefund;
using AutoRefund.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.AutoRefund.Service
{
    public partial class Startup : IHostedService
    {
        // private readonly MaintainanceService maintainanceService;

        private readonly IAutoRefundController autoRefundController;

        private readonly IMatchService matchService;
        public IConfiguration configuration { get; }

        private bool running;

        private int counter;

        private int timeCounter;

        public Startup(IAutoRefundController autoRefundController, IConfiguration configuration, IMatchService matchService)
        {
            this.autoRefundController = autoRefundController;
            this.matchService = matchService;
            //this.maintainanceService = maintainanceService;
            this.configuration = configuration;
            InitializeExtraComponent();
            InitializeSetting();
        }

        private void InitializeSetting()
        {
            UMSetting();
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
                Logging.LogService.Error(x.Message, x);
                return true;
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            running = true;
            var autoRefundThread = new Thread(RefundThreadProcess);
            Console.WriteLine($"AutoRefund Starting at { DateTime.Now }");
            autoRefundThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            running = false;
            Console.WriteLine($"AutoRefund Stopping at { DateTime.Now }");
            return Task.CompletedTask;
        }

        private void RefundThreadProcess()
        {
            while (running)
            {
                //if (!maintainanceService.IsMaintainance)
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
                handleMissedMatch();
                HandleGarbageCollector();

            }
            catch (Exception exception)
            {
                Logging.LogService.Error("Exception in main thread - Process:", exception);
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

        private void handleMissedMatch()
        {
            if (timeCounter >= int.Parse(configuration.GetSection("CheckMissedMatchInterval").Value))
            {
                timeCounter = 0;
                matchService.CheckMissedMatch();
            }

            timeCounter += int.Parse(configuration.GetSection("ServiceInterval").Value);

        }
    }
}