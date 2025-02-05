namespace AutoEarlyRefund;

public enum SettleStatus
{
    Running,
    Refunded = 1,
    Calculated = 4,
}


public enum ResultStatus
{
    AutoSettled = 2,
    ManualSettled = 5
}