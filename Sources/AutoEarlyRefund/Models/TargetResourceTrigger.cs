namespace AutoEarlyRefund.Models;

public class TargetResourceTrigger
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string Action { get; set; }
    public int MatchType { get; set; }
    public short ChildMatchType { get; set; }
    public string Conditions { get; set; }
    public string TargetResource { get; set; }
}

