using Fanex.Logging.Sentry.RavenClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoEarlyRefund.UnitTests;

public abstract class TestBase : IDisposable
{
    private readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"LogCategory", "NAP-Settlement-LOCAL"},
                {"SentryEngineOptions:Dsn", "http://2dfee6c5bbc844558ad3bef5f4b59ee1@sentry.nexdev.net/9"}
            }!)
            .Build();

        var services = new ServiceCollection();

        services.AddFanexLogSentry(configuration);

        ServiceProvider = services.BuildServiceProvider();
    }

    // Helper method để lấy service
    protected T GetService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}