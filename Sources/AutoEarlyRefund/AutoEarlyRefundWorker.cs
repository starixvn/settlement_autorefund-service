namespace AutoEarlyRefund;

public class AutoEarlyRefundWorker : BackgroundService
{
    private readonly IConfiguration configuration;

    public AutoEarlyRefundWorker(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(configuration.GetValue<int>("ServiceInterval")));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            //await HandleAutoAsync();
        }
    }
}
