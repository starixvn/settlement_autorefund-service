using System.Text.Json;
using AutoEarlyRefund.Features.GetResourcesConditions;

namespace AutoEarlyRefund.Models;
public class ResultDetail
{
    public int MatchId { get; set; }
    public int LeagueId { get; set; }
    public short SportType { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string HomeValue { get; set; } = string.Empty;
    public string AwayValue { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public short ChildMatchType { get; set; }
    public string OtherInfo { get; set; } = string.Empty;

    public SourcesConditionsItem? Conditions
        => JsonSerializer.Deserialize<SourcesConditionsItem>(OtherInfo, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

    public override string ToString() => $"{ResourceId}({HomeValue}-{AwayValue})";
}
