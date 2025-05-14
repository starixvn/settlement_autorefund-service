namespace AutoEarlyRefund.Features.GetResourcesInfo;

public class ResourceInfoItemRequest
{
    public short SportType { get; set; }
    public short SourceMatchType { get; set; }
    public short SourceChildMatchType { get; set; }
    public string SourceResource { get; set; }
    public int MatchId { get; set; }
}
