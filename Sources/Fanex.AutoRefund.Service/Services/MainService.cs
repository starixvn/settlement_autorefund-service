using AutoRefund.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.AutoRefund.Service
{
    public class MainService : IHostedService
    {
        private readonly MaintainanceService maintainanceService;

        private readonly IAutoRefundController autoRefundController;

        private bool running;

        private int counter;

        public MainService(IAutoRefundController autoRefundController)
        {
            this.autoRefundController = autoRefundController;
            this.maintainanceService = maintainanceService;
            InitializeExtraComponent();
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
                    Thread.Sleep(AppSettingKeys.ServiceInterval);
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
            if (counter >= AppSettingKeys.GarbageCollectorInterval)
            {
                counter = 0;
                GC.WaitForPendingFinalizers();
            }

            counter += AppSettingKeys.ServiceInterval;
        }
    }
}