using Fanex.Data.Repository;

namespace AutoRefund.Core.Criteria;
public sealed class GetAutoRefundResultsCriteria : CriteriaBase
{
    private const string SettingKey = "NAP_Set_AutoRefundResult_Select";

    public string ResourceInfo { get; init; } = string.Empty;

    public override string GetSettingKey() => SettingKey;

    public override bool IsValid() => !string.IsNullOrWhiteSpace(ResourceInfo);
}
