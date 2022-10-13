using AutoRefund.Models;

namespace AutoRefund;

public static class LeagueSettingFactory
{
    private static FileSystemWatcher watcher = new FileSystemWatcher();

    private static LeagueSetting leagueSetting = InitLeagueSetting();

    private const int SleepTime = 2000;

    private static LeagueSetting InitLeagueSetting()
    {
        return ReadFileSetting();
    }

    public static void SettingFileWatcher()
    {
        watcher.Path = AppDomain.CurrentDomain.BaseDirectory;
        watcher.NotifyFilter = NotifyFilters.LastAccess |
                               NotifyFilters.LastWrite |
                               NotifyFilters.FileName |
                               NotifyFilters.DirectoryName |
                               NotifyFilters.Size;
        watcher.Filter = "auto-league-settings.ini";
        watcher.Changed += OnChanged;
        watcher.Error += WatcherError;
        watcher.EnableRaisingEvents = true;
    }

    private static void OnChanged(object source, FileSystemEventArgs e)
    {
        leagueSetting = ReadFileSetting();
    }

    private static void WatcherError(object source, ErrorEventArgs e)
    {
        watcher = new FileSystemWatcher();
        while (!watcher.EnableRaisingEvents)
        {
            try
            {
                SettingFileWatcher();
            }
            catch
            {
                Thread.Sleep(SleepTime);
            }
        }
    }

    public static LeagueSetting Create()
    {
        return leagueSetting ?? new LeagueSetting();
    }

    private static LeagueSetting ReadFileSetting()
    {
        try
        {
            FileStream fileStream = new FileStream(@"auto-league-settings.ini", FileMode.Open);
            using (var streamReader = new StreamReader(fileStream))
            {
                return new LeagueSetting
                {
                    ExcludeLeagueIds = GetLeagueIds("ExcludeLeagueIds:", streamReader.ReadLine()),
                    IncludeLeagueIds = GetLeagueIds("IncludeLeagueIds:", streamReader.ReadLine())
                };
            }
        }
        catch
        {
            return new LeagueSetting();
        }
    }

    private static List<int> GetLeagueIds(string key, string text)
    {
        var content = text.Replace(key, string.Empty).Trim();
        return string.IsNullOrWhiteSpace(content)
            ? new List<int>()
            : content.Split(',').Select(int.Parse).ToList();
    }
}