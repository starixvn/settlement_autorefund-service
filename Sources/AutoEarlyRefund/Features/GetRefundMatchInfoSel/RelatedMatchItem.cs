namespace AutoEarlyRefund.Features.GetRefundMatchInfoSel;

public class RelatedMatchItem
{
    public int MatchId { get; set; }
    public short ChildMatchType { get; set; }
    public short TargetChildMatchType { get; set; }
    public int TargetMatchId { get; set; }
}
