using System.Collections.Generic;

namespace AutoRefund.Models
{
    public class AppSettingsKey
    {
        public int RetryInterval { get; set; }

        public int RetryCount { get; set; }

        public string EnvironmentName { get; set; }

        public List<string> SiteDbServer { get; set; }
    }
}