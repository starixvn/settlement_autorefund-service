using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoEarlyRefund.Features.GetResourcesConditions;

public class SourcesConditionsItem
{
    public string ResourceId { get; set; } = string.Empty;
    public short SportType { get; set; }
    public short ChildMatchType { get; set; }
    public short MatchType { get; set; }
    public short RuleId { get; set; }
    public bool IsRefundSdt { get; set; }


    public SourcesConditionsItem WithMatchType(short matchType)
    {
        MatchType = matchType;
        return this;
    }

    public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });
}
