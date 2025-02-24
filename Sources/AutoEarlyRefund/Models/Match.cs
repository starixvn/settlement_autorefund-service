namespace AutoEarlyRefund.Models;

public class Match
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public short SportType { get; set; }
    public short ChildMatchType { get; set; }
    public List<ResultDetail> ResultDetails { get; set; } = [];
    public string ResourcesXml { get; set; } = "";

    public Match WithResourcesXml(XmlConvertService xmlConvertService, bool hasOtherInfo = true)
    {
        var selectedResources = ResultDetails
            .Select(x => new ResourceItem
            {
                ResourceId = x.ResourceId,
                HomeValue = x.HomeValue,
                AwayValue = x.AwayValue,
                Remark = x.Remark,
                SourceId = x.SourceId,
                OtherInfo = hasOtherInfo ? x.OtherInfo : string.Empty,
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
               && match.MatchId == MatchId && match.ChildMatchType == ChildMatchType;
    }

    public override int GetHashCode()
    {
        return MatchId ^ ChildMatchType;
    }
}
