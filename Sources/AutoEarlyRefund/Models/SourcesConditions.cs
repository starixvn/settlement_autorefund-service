namespace AutoEarlyRefund.Models;

public class SourcesConditions
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string ResourceId { get; set; }
    public string RuleName { get; set; }

    public string Action { get; set; }
    public short MatchType { get; set; }
    public short ChildMatchType { get; set; }
    public short SourceMatchType { get; set; }
    public short SourceChildMatchType { get; set; }
    public string SourceResource { get; set; }
    public string Conditions { get; set; }
}
