using System.Text.Json;

namespace AutoEarlyRefund.Contracts;
public record MatchCalculated(
    int MatchId,
    int ParentId,
    short ShortType,
    short ChildMatchType,
    int? HalfTimeHomeScore,
    int? HalfTimeAwayScore,
    int? FinalHomeScore,
    int? FinalAwayScore,
     string Action = "Cal"
    );

public record MainMatchCalculated(
    int MatchId,
    int ParentId,
    short ShortType,
    short ChildMatchType,
    int? HalfTimeHomeScore,
    int? HalfTimeAwayScore,
    int? FinalHomeScore,
    int? FinalAwayScore,
    string Action = "Cal"
);


public record ExtraMatchCalculated(
    int MatchId,
    int ParentId,
    short ShortType,
    short ChildMatchType,
    int? HalfTimeHomeScore,
    int? HalfTimeAwayScore,
    int? FinalHomeScore,
    int? FinalAwayScore,
    string Action = "Cal"
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
    )
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}

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
    )
{
    public override string ToString()
       => JsonSerializer.Serialize(this);
}

