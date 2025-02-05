namespace AutoEarlyRefund.Contracts;
public record MatchCalculated(
    int MatchId,
    int Action,
    int EventId,
    string HomeValue,
    string AwayValue);

public record MatchExtraCalculated(
    int MatchId,
    int Action,
    int EventId,
    string HomeValue,
    string AwayValue);

public record MatchPenCalculated(
    int MatchId,
    int Action,
    int EventId,
    string HomeValue,
    string AwayValue);

public record MatchRefunded(
    int MatchId,
    int Action,
    int EventId);

public record ResultsCalculated(
    int MatchId,
    int Action,
    List<ResourceItem> Resources);

public record ResultsRefunded(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    string Action,
    List<ResourceItem> Resources
    );

public record ResultsRefundConfirmed(
    );


public class ResourceItem
{
    public string ResourceId { get; set; }
    public string HomeValue { get; set; }
    public string AwayValue { get; set; }
}
