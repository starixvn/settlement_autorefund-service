namespace AutoEarlyRefund.Features.GetResourcesConditions;

public class SourcesConditionsItemRequest
{
    public string ResourceId { get; set; } = string.Empty;
    public short SportType { get; set; }
    public short childMatchType { get; set; }
    public short MatchType { get; set; }
    public short RuleId { get; set; }
}
