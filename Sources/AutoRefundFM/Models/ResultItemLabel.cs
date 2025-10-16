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

    public static ResultItemLabel TimeMachineHdp() => new() { None = "Home", Goal = "Away" };

    public static ResultItemLabel Create(int betType, string odds) => betType switch
    {
        FastMarketBetType.NextFiveMinutes => NextFiveMinutes(odds),
        FastMarketBetType.NextOneMinuteSetPiece => NextOneMinuteSetPiece(),
        FastMarketBetType.TimeMachineHdp => TimeMachineHdp(),
        _ => new ResultItemLabel()
    };

    private static ResultItemLabel NextOneMinuteSetPiece() =>
    new()
    { None = "No", Goal = "Yes <a title='Corner, Free Kick or Throw In'>(!)</a>" };

    private static ResultItemLabel NextFiveMinutesDefault() =>
        new()
        {
            IsSetDefaultValue = true,
            Goal = "0.5",
            Corner = "0.5",
            FreeKick = "1.5",
            GoalKick = "0.5",
            ThrowIn = "2.5",
            Penalty = "0.5"
        };

    private static ResultItemLabel NextFiveMinutes(string odd)
    {
        if (string.IsNullOrWhiteSpace(odd))
        {
            return NextFiveMinutesDefault();
        }

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

    private static string ResultName(string[] name, int index) => name.Length > 0 ? name[index].Split(':')[1] : string.Empty;
}