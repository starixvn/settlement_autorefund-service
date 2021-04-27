namespace AutoRefund.Models
{
    public class AppSettingsKey
    {
        public int RetryInterval { get; set; }

        public int RetryCount { get; set; }

        public int WaitingParlay { get; set; }

        public string EnvironmentName { get; set; }
    }
}