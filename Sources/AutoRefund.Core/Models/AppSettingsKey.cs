namespace AutoRefund.Core.Models;

public class AppSettingsKey
{
    public int RetryInterval { get; set; }

    public int RetryCount { get; set; }

    public string EnvironmentName { get; set; }

    public List<string> SiteDbServer { get; set; }

    public int RetryProcessScore { get; set; }
}