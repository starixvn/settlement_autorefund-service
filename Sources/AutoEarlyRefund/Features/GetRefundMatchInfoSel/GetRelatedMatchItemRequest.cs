namespace AutoEarlyRefund.Features.GetRefundMatchInfoSel;

public class GetRelatedMatchItemRequest
{
    public int MatchId { get; set; }
    public int ChildMatchType { get; set; }
    public int TargetChildMatchType { get; set; }
}
