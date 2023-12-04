namespace AutoRefund.Models;

public class MatchChecking
{
    private const int ToBase = 2;

    private const int Calculated = 4;

    private const string Settled = "completed";

    public byte SettlementStatus { get; set; }

    public RemarkSettingModel Remark
    {
        get
        {
            char[] status = Convert.ToString(SettlementStatus, ToBase).ToCharArray();
            Array.Reverse(status);
            return new RemarkSettingModel
            {
                SettlementStatus = new string(status),
            };
        }
        set => throw new NotImplementedException();
    }

    public int ParentSettlementStatusId { get; set; }

    public int SettlementStatusId { get; set; }

    public string EventStatus { get; set; }

    public bool IsMainMatchSettled
    {
        get => ParentSettlementStatusId == Calculated;
        set => throw new NotImplementedException();
    }

    public bool IsSettled
    {
        get => !string.IsNullOrWhiteSpace(EventStatus) &&
               EventStatus.Equals(Settled, StringComparison.OrdinalIgnoreCase);
        set => throw new NotImplementedException();
    }

    public bool IsRefundedSingle { get; set; }

    public bool IsHoldResourceId { get; set; }

    public int ParentHomeValue { get; set; }

    public int ParentAwayValue { get; set; }
}