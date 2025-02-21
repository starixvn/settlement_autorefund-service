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

public enum MatchType
{
    Main = 1,
    Child = 2,
    BaseballSeries = 3
}