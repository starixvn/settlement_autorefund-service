using System.Text.Json;

namespace AutoEarlyRefund.Models;

public class SourcesConditions
{
    public int Id { get; set; }
    public short SportType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public short ChildMatchType { get; set; }
    public short MatchType { get; set; }
    public int RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;

    public string SourceAction { get; set; } = string.Empty;
    public short SourceMatchType { get; set; }
    public short SourceChildMatchType { get; set; }
    public string SourceResource { get; set; } = string.Empty;
    public string SourceResourceId { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;

    public Resource Resource { get; set; }

    public IEnumerable<string> SourceResourceIds => SourceResource.Split(',');

    public override string ToString()
        => JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
}