namespace AutoEarlyRefund.Models;

public class TargetResourceTrigger
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string Action { get; set; } = string.Empty;
    public short MatchType { get; set; }
    public short ChildMatchType { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public bool TargetIsCrrMatch { get; set; }
    public bool TargetIsCrrResource { get; set; }
    public string TargetResourceChildMatchType { get; set; } = string.Empty;
    public string TargetResource { get; set; } = string.Empty;

    public List<string> TargetResourceIds => !string.IsNullOrEmpty(TargetResource)
            ? TargetResource.Split(',').ToList()
            : [];
}

