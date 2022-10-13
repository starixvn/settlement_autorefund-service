namespace AutoRefund.Models;

public class LeagueSetting
{
    public LeagueSetting()
    {
        ExcludeLeagueIds = new List<int>();
        IncludeLeagueIds = new List<int>();
    }

    public List<int> ExcludeLeagueIds { get; set; }

    public List<int> IncludeLeagueIds { get; set; }
}
