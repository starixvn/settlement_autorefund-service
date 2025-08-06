namespace AutoRefundFM.Models;
using Fanex.Settlement.Converter.Helpers;
public class FastMarketResultDetail
{
    private const int TenMinutes = 10;

    private const int FirstHalfInjuryTime = 45;

    private const int TimeMachineHomeAwayRange = 10;

    private ResultItem resultItem;

    private int result;

    public int BetType { get; set; }

    public int Position { get; set; }

    public string PositionName
    {
        get
        {
            switch (BetType)
            {
                case FastMarketBetType.NextOneMinute:
                case FastMarketBetType.FirstNextOneMinute:
                case FastMarketBetType.NextOneMinuteSetPiece:
                case FastMarketBetType.CombinationNextOneMinute:
                    return OneMinuteInterval(Position);

                case FastMarketBetType.TimeMachineMinutes:
                case FastMarketBetType.TimeMachineHdp:
                    {
                        var interval = Position > FirstHalfInjuryTime ? Position - 1 : Position;
                        return Position == FirstHalfInjuryTime ? "First Half Injury Time" : TimeMachineInterval(interval);
                    }
                default:
                    return FiveMinutesInterval(Position);
            }
        }
    }

    public int? Result
    {
        get =>
            BetType == FastMarketBetType.NextOneMinuteSetPiece &&
            result == SetPiece.CornerFreeKickGoalKickAndPenalty
                ? PositionResult.Corner
                : result;

        set => result = value ?? 0;
    }

    public int ResultStatus { get; set; }

    public string ResultStatusName => Status.GetResultStatusNameById(ResultStatus);

    public int TicketStatus { get; set; }

    public string TicketStatusName => Status.GetTicketStatusNameById(TicketStatus);

    public bool Checked { get; set; }

    public bool IsShow { get; set; }

    public ResultItem ResultItem
    {
        get
        {
            if (Result >= 0 && !Checked)
            {
                var binaryResult = Convert.ToString((int)Result, 2).PadLeft(8, '0');
                resultItem = ResultMapping(binaryResult.Reverse());
            }

            return resultItem;
        }

        set => resultItem = value;
    }

    private static string OneMinuteInterval(int position)
    {
        return position < TenMinutes ? $"0{position}:00-0{position}:59" : $"{position}:00-{position}:59";
    }

    private static string TimeMachineInterval(int position)
    {
        const int nineMinute = 9;

        return position switch
        {
            0 => $"0{position}:00-01:00",
            < nineMinute => $"0{position}:01-0{position + 1}:00",
            nineMinute => $"0{position}:01-{position + 1}:00",
            _ => $"{position}:01-{position + 1}:00"
        };
    }

    private static string FiveMinutesInterval(int position)
    {
        var startTime = position < TenMinutes ? $"0{position}:00" : $"{position}:00";

        var endPosition = position + 4;
        var endTime = endPosition < TenMinutes ? $"0{endPosition}:59" : $"{endPosition}:59";

        return $"{startTime}-{endTime}";
    }

    private static ResultItem ResultMapping(string binaryResult)
    {
        var item = new ResultItem();
        for (var pos = 0; pos < binaryResult.Length; pos++)
        {
            var resultPosition = binaryResult[pos].ToString();
            var value = !resultPosition.Equals("0");
            switch (pos)
            {
                case PositionResult.None:
                    item.None = value;
                    break;

                case PositionResult.Goal:
                    item.Goal = value;
                    break;

                case PositionResult.Corner:
                    item.Corner = value;
                    break;

                case PositionResult.FreeKick:
                    item.FreeKick = value;
                    break;

                case PositionResult.GoalKick:
                    item.GoalKick = value;
                    break;

                case PositionResult.ThrowIn:
                    item.ThrowIn = value;
                    break;

                case PositionResult.Booking:
                    item.Bookings = value;
                    break;

                case PositionResult.Penalty:
                    item.Penalty = value;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected value position = " + pos);
            }
        }

        return item;
    }

    public int HomeGoal
    {
        get
        {
            if (BetType == FastMarketBetType.TimeMachineHdp && Result != null)
            {
                return (int)Result % TimeMachineHomeAwayRange;
            }

            return 0;
        }
    }

    public int AwayGoal
    {
        get
        {
            if (BetType == FastMarketBetType.TimeMachineHdp && Result != null)
            {
                return (int)Result / TimeMachineHomeAwayRange;
            }

            return 0;
        }
    }

    public int TicketCount { get; set; }

    public int TicketRunning { get; set; }

    public int TicketMarked { get; set; }

    public string SourceId { get; set; }

    public override bool Equals(object obj)
    {
        return obj is FastMarketResultDetail fastMarketResult && fastMarketResult.BetType == BetType && fastMarketResult.Position == Position;
    }

    public override int GetHashCode()
    {
        return BetType * Position;
    }
}
