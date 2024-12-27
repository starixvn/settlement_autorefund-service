using AutoRefund.Core.Models;
using Fanex.Settlement.Converter.Services;

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
                AwayValue = x.AwayValue
            }).ToList();

        var resources = new Resources<ResourceItem>
        {
            ResourceItems = selectedResources
        };

        var xml = xmlConvertService.Serialize(resources);

        ResourcesXml = xml;

        return this;
    }
}
