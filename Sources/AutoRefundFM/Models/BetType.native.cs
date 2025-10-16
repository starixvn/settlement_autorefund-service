namespace AutoRefundFM.Models;
public class BetType
{

    public int BetTypeId { get; set; }

    public string BetTypeName { get; set; } = string.Empty;

    public ResultItemLabel ResultName { get; set; } = new ResultItemLabel();

    public bool Checked { get; set; }

    public List<FastMarketResultDetail> FastMarketResults { get; set; } = [];
}


