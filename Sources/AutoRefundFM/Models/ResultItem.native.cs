using Fanex.Settlement.Converter.Helpers;

namespace AutoRefundFM.Models;
public class ResultItem
{
    public bool None { get; set; }

    public bool Goal { get; set; }

    public bool Corner { get; set; }

    public bool FreeKick { get; set; }

    public bool GoalKick { get; set; }

    public bool ThrowIn { get; set; }

    public bool Bookings { get; set; }

    public bool Penalty { get; set; }

    public int Value
    {
        get
        {
            var result = new string[8];
            result[PositionResult.None] = None.ToIntString();
            result[PositionResult.Goal] = Goal.ToIntString();
            result[PositionResult.Corner] = Corner.ToIntString();
            result[PositionResult.FreeKick] = FreeKick.ToIntString();
            result[PositionResult.GoalKick] = GoalKick.ToIntString();
            result[PositionResult.ThrowIn] = ThrowIn.ToIntString();
            result[PositionResult.Booking] = Bookings.ToIntString();
            result[PositionResult.Penalty] = Penalty.ToIntString();

            var choiceValue = string.Join(string.Empty, result).Reverse();
            const int convertToBase = 2;
            return Convert.ToInt32(choiceValue, convertToBase);
        }
    }
}