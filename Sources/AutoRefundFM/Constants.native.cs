namespace AutoRefundFM;
public static class FastMarketBetType
{
    public const int NextOneMinute = 221;

    public const int NextFiveMinutes = 222;

    public const int FirstNextOneMinute = 223;

    public const int NextOneMinuteSetPiece = 225;

    public const int CombinationNextOneMinute = 226;

    public const int TimeMachineMinutes = 228;

    public const int TimeMachineHdp = 229;

    public static int[] RadioResults = [FirstNextOneMinute, NextOneMinuteSetPiece, CombinationNextOneMinute];
}

public static class PositionResult
{
    public const int None = 0;
    public const int Goal = 1;
    public const int Corner = 2;
    public const int FreeKick = 3;
    public const int GoalKick = 4;
    public const int ThrowIn = 5;
    public const int Booking = 6;
    public const int Penalty = 7;
}

public static class TicketStatus
{
    public const string NoTicket = "No Ticket";

    public const string Running = "Running";

    public const string Marked = "Marked";

    public const string Waiting = "Waiting";

    public const string Calculated = "Calculated";

    public const string Completed = "Completed";

    public const string Refunded = "Refunded";

    public const string NotIncluded = "Invalid";

    public const string NotIncludedOnSoccer = "Not Included (for get match)";

    public const string Normal = "N/A";
}

public static class SetPiece
{
    public const int CornerFreeKickGoalKickAndPenalty = 44;
}

public static class Message
{

}

public static class EventCounterResource
{
    public const string Goal = "221CNT02";
    public const string Corner = "221CNT03";
    public const string FreeKick = "221CNT04";
    public const string GoalKick = "221CNT05";
    public const string ThrowIn = "221CNT06";
    public const string Penalty = "221CNT08";
}

public static class FastMarketErrorCodes
{
    public const int NoCompletedResults = 101;
    public const int ExistingRefundsFound = 102;
    public const int EventTicketValidationFailed = 103;
    public const int MarkedTicketsFound = 104;
    public const int InvalidChoiceResult = 105;
    public const int NoEventCounters = 106;
    public const int EventCounterValidationFailed = 107;

}