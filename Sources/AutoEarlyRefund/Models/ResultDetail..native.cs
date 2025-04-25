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
    public int ResultStatusId { get; set; }
    public int StatusId { get; set; }
    public int SettlementStatusId { get; set; }

    public SourcesConditionsItem? Conditions
        => string.IsNullOrWhiteSpace(OtherInfo)
            ? null
            : JsonSerializer.Deserialize<SourcesConditionsItem>(OtherInfo, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

    public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    public override bool Equals(object? obj) => obj is ResultDetail detail && MatchId == detail.MatchId && ResourceId == detail.ResourceId;
    public override int GetHashCode() => HashCode.Combine(MatchId, ResourceId);


    public ResultDetail WithRemark(string remark)
    {
        Remark = remark;
        return this;
    }
}
