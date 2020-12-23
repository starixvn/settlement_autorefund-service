using AutoRefund;
using AutoRefund.Constants;
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
        private readonly MaintainanceService maintainanceService;

        private readonly IAutoRefundController autoRefundController;
        public IConfiguration configuration { get; }

        private bool running;

        private int counter;

        public Startup(IAutoRefundController autoRefundController, IConfiguration configuration)
        {
            this.autoRefundController = autoRefundController;
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
            autoRefundThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            running = false;
            return Task.CompletedTask;
        }

        private void RefundThreadProcess()
        {
            while (running)
            {
                //if (!maintainanceService.IsMaintainance)
                {
                    MainProcess();
                    Thread.Sleep(SettingsKey.ServiceInterval);
                }
            }
        }

        private void MainProcess()
        {
            try
            {
                autoRefundController.ProcessAsync();
                HandleGarbageCollector();
            }
            catch (Exception exception)
            {
                Logging.LogService.Error("Exception in main thread - Process:", exception);
            }
        }

        private void HandleGarbageCollector()
        {
            if (counter >= SettingsKey.GarbageCollectorInterval)
            {
                counter = 0;
                GC.WaitForPendingFinalizers();
            }

            counter += SettingsKey.ServiceInterval;
        }
    }
}