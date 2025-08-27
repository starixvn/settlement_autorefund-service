using System;

namespace AutoRefundFM.Models;

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

    // Loại NextOneMinuteSetPiece
    public static ResultItemLabel NextOneMinuteSetPiece() =>
        new ResultItemLabel { None = "No", Goal = "Yes <a title='Corner, Free Kick or Throw In'>(!)</a>" };

    // Loại NextFiveMinutes mặc định
    public static ResultItemLabel NextFiveMinutesDefault() =>
        new ResultItemLabel
        {
            IsSetDefaultValue = true,
            Goal = "0.5",
            Corner = "0.5",
            FreeKick = "1.5",
            GoalKick = "0.5",
            ThrowIn = "2.5",
            Penalty = "0.5"
        };

    // Loại NextFiveMinutes từ chuỗi odd
    public static ResultItemLabel NextFiveMinutes(string odd)
    {
        if (string.IsNullOrWhiteSpace(odd))
            return NextFiveMinutesDefault();

        var name = odd.Split(',');
        return new ResultItemLabel
        {
            Goal = ResultName(name, PositionResult.Goal),
            Corner = ResultName(name, PositionResult.Corner),
            FreeKick = ResultName(name, PositionResult.FreeKick),
            GoalKick = ResultName(name, PositionResult.GoalKick),
            ThrowIn = ResultName(name, PositionResult.ThrowIn),
            Penalty = ResultName(name, PositionResult.Penalty)
        };
    }

    // Loại TimeMachineHdp
    public static ResultItemLabel TimeMachineHdp() =>
        new ResultItemLabel { None = "Home", Goal = "Away" };

    public static ResultItemLabel Create(int betType, string odds)
    {
        return betType switch
        {
            FastMarketBetType.NextFiveMinutes => NextFiveMinutes(odds),
            FastMarketBetType.NextOneMinuteSetPiece => NextOneMinuteSetPiece(),
            FastMarketBetType.TimeMachineHdp => TimeMachineHdp(),
            _ => new ResultItemLabel()
        };
    }
    private static string ResultName(string[] name, int index)
    {
        return name.Length > 0 ? name[index].Split(':')[1] : string.Empty;
    }
}