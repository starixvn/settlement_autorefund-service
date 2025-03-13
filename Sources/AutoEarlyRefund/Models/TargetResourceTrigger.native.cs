namespace AutoEarlyRefund.Models;

public class TargetResourceTrigger
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string Action { get; set; } = string.Empty;
    public short MatchType { get; set; }
    public short ChildMatchType { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public int TargetIsCrrMatch { get; set; }
    public int TargetIsCrrResource { get; set; }
    public int TargetResourceChildMatchType { get; set; }
    public string TargetResource { get; set; } = string.Empty;

    public string[] TargetResourceIds => !string.IsNullOrEmpty(TargetResource)
            ? TargetResource.Split(',')
            : [];
}

