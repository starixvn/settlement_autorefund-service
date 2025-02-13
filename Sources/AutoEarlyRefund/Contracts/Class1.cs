namespace AutoEarlyRefund.Contracts;
public record MatchCalculated(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    string Action,
    int? HalfTimeHomeScore,
    int? HalfTimeAwayScore,
    int? FinalHomeScore,
    int? FinalAwayScore
    );


public record ResultsCalculated(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "Cal");

public record ResultsRefunded(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "Refund"
    );

public record ResultsRefundConfirmed(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "CfnRefund"
    );

public record ResultsRefundConfirmedSdt(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "CfnRefundSDT"
    );

