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
    )
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}

public record ResultsCalculated(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "Cal")
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}

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
    string Action = "CfmRefund"
    )
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}

public record NotTakenResultsConfirmed(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "CfmRefundAdminSite"
    )
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}

public record ResultsRefundConfirmedSdt(
    int MatchId,
    short ShortType,
    short ChildMatchType,
    List<ResourceItem> Resources,
    string Action = "CfmRefundSDT"
    )
{
    public override string ToString()
       => JsonSerializer.Serialize(this);
}


public record MatchRefunded(
    int MatchId,
    int ParentId,
    short ShortType,
    short ChildMatchType,
    string ActionName,
    string Action = "Refund"
    )
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}

