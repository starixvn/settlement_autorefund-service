namespace AutoEarlyRefund.Features.GetResourcesConditions;

public class SourcesConditionsItem
{
    public string ResourceId { get; set; } = string.Empty;
    public short SportType { get; set; }
    public short ChildMatchType { get; set; }
    public short MatchType { get; set; }
    public short RuleId { get; set; }


    public SourcesConditionsItem WithMatchType(short matchType)
    {
        MatchType = matchType;
        return this;
    }

}
