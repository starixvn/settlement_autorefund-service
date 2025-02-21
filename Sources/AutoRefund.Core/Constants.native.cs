namespace AutoRefund.Core;

public static class ActionName
{
    public const string Refund = "REFUND";
}

public static class ResultRefundStatus
{
    public const int Initial = 0;

    public const int Running = 1;

    public const int Completed = 2;

    public const int Failed = 3;
}

public static class CacheKey
{
    public const string LeagueId = "Settlement-AutoPayout-AutoInstant-LeagueId";
}

public static class ResourceId
{
    public const string Halftime = "01";

    public const string Final = "00";
}