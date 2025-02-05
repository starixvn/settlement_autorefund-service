namespace AutoEarlyRefund.Models;

public class Match
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public List<ResultDetail> ResultDetails { get; set; } = [];
    public string ResourcesXml { get; set; } = "";

    public Match WithResourcesXml(XmlConvertService xmlConvertService)
    {
        var selectedResources = ResultDetails
            .Select(x => new ResourceItem
            {
                ResourceId = x.ResourceId,
                HomeValue = x.HomeValue,
                AwayValue = x.AwayValue,
                Remark = x.Remark
            }).ToList();

        var resources = new Resources<ResourceItem>
        {
            ResourceItems = selectedResources
        };

        var xml = xmlConvertService.Serialize(resources);

        ResourcesXml = xml;

        return this;
    }


    public override bool Equals(object obj)
    {
        return (obj is Match match)
               && match.MatchId == MatchId;
    }

    public override int GetHashCode()
    {
        return MatchId;
    }
}
