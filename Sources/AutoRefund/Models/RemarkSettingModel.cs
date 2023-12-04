namespace AutoRefund.Models;

public class RemarkSettingModel
{
    private const int HoldKeyPosition = 6;

    private const int MtdHoldPosition = 4;

    private const int UndecidedPosition = 1;

    private const int UnderSettlePosition = 2;

    private const int RefundAllPosition = 5;

    public string SettlementStatus { get; set; }

    public bool IsPending
    {
        get => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus[0].Equals('1');
        set => throw new NotImplementedException();
    }

    public bool IsUndecided
    {
        get => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > UndecidedPosition &&
               SettlementStatus[UndecidedPosition].Equals('1');
        set => throw new NotImplementedException();
    }

    public bool IsMtdHold
    {
        get => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > MtdHoldPosition &&
               SettlementStatus[MtdHoldPosition].Equals('1');
        set => throw new NotImplementedException();
    }

    public bool IsRefundAll
    {
        get => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > RefundAllPosition &&
               SettlementStatus[RefundAllPosition].Equals('1');
        set => throw new NotImplementedException();
    }

    public bool IsHoldKey
    {
        get => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > HoldKeyPosition &&
               SettlementStatus[HoldKeyPosition].Equals('1');
        set => throw new NotImplementedException();
    }

    public bool IsUnderSettle
    {
        get => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > UnderSettlePosition &&
               SettlementStatus[UnderSettlePosition].Equals('1');
        set => throw new NotImplementedException();
    }
}