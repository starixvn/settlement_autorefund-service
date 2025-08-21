namespace AutoRefundFM.Models;
public class BetType
{

    public int BetTypeId { get; set; }

    public string BetTypeName { get; set; } = string.Empty;

    public ResultItemLabel ResultName { get; set; } = new ResultItemLabel();

    public bool Checked { get; set; }

    public List<FastMarketResultDetail> FastMarketResults { get; set; } = [];
}


public class ResultItemLabel
{
    public bool IsSetDefaultValue { get; set; }

    public string None { get; set; } = string.Empty;

    public string Goal { get; set; } = string.Empty;

    public string Corner { get; set; } = string.Empty;

    public string FreeKick { get; set; } = string.Empty;

    public string GoalKick { get; set; } = string.Empty;

    public string ThrowIn { get; set; } = string.Empty;

    public string Penalty { get; set; } = string.Empty;
}