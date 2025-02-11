namespace AutoEarlyRefund.Models;

public class SourcesConditions
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public short ChildMatchType { get; set; }
    public short MatchType { get; set; }
    public string RuleName { get; set; } = string.Empty;

    public string SourceAction { get; set; }
    public short SourceMatchType { get; set; }
    public short SourceChildMatchType { get; set; }
    public string SourceResource { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;

    public Resource Resource { get; set; }

    public override string ToString() => $"{ResourceId}-{RuleName}, Target: {SourceResource}";
}
