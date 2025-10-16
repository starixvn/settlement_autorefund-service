using AutoRefundFM.Models;

namespace AutoRefundFM.UnitTests.Models;

public class ResultItemLabelTests
{
    [Fact]
    public void Create_NextFiveMinutes_WithOddsString_ParsesCorrectly()
    {
        // Arrange
        var odds = "1:,2:0.5,3:0.5,4:1.5,5:0.5,6:2.5,7:,8:0.5";

        // Act
        var label = ResultItemLabel.Create(FastMarketBetType.NextFiveMinutes, odds);

        // Assert
        Assert.Equal("0.5", label.Goal);
        Assert.Equal("0.5", label.Corner);
        Assert.Equal("1.5", label.FreeKick);
        Assert.Equal("0.5", label.GoalKick);
        Assert.Equal("2.5", label.ThrowIn);
        Assert.Equal("0.5", label.Penalty);
        Assert.False(label.IsSetDefaultValue);
    }

    [Fact]
    public void Create_NextFiveMinutes_WithEmptyOdds_UsesDefault()
    {
        // Act
        var label = ResultItemLabel.Create(FastMarketBetType.NextFiveMinutes, "");

        // Assert
        Assert.Equal("0.5", label.Goal);
        Assert.Equal("0.5", label.Corner);
        Assert.Equal("1.5", label.FreeKick);
        Assert.Equal("0.5", label.GoalKick);
        Assert.Equal("2.5", label.ThrowIn);
        Assert.Equal("0.5", label.Penalty);
        Assert.True(label.IsSetDefaultValue);
    }

    [Fact]
    public void Create_NextOneMinuteSetPiece_ReturnsExpectedLabels()
    {
        // Act
        var label = ResultItemLabel.Create(FastMarketBetType.NextOneMinuteSetPiece, "ignored");

        // Assert
        Assert.Equal("No", label.None);
        Assert.StartsWith("Yes", label.Goal);
        Assert.Equal(string.Empty, label.Corner);
        Assert.Equal(string.Empty, label.FreeKick);
        Assert.Equal(string.Empty, label.GoalKick);
        Assert.Equal(string.Empty, label.ThrowIn);
        Assert.Equal(string.Empty, label.Penalty);
    }

    [Fact]
    public void Create_TimeMachineHdp_ReturnsExpectedLabels()
    {
        // Act
        var label = ResultItemLabel.Create(FastMarketBetType.TimeMachineHdp, "ignored");

        // Assert
        Assert.Equal("Home", label.None);
        Assert.Equal("Away", label.Goal);
        Assert.Equal(string.Empty, label.Corner);
        Assert.Equal(string.Empty, label.FreeKick);
        Assert.Equal(string.Empty, label.GoalKick);
        Assert.Equal(string.Empty, label.ThrowIn);
        Assert.Equal(string.Empty, label.Penalty);
    }

    [Fact]
    public void Create_UnknownBetType_ReturnsEmptyLabels()
    {
        // Act
        var label = ResultItemLabel.Create(999, "ignored");

        // Assert
        Assert.Equal(string.Empty, label.None);
        Assert.Equal(string.Empty, label.Goal);
        Assert.Equal(string.Empty, label.Corner);
        Assert.Equal(string.Empty, label.FreeKick);
        Assert.Equal(string.Empty, label.GoalKick);
        Assert.Equal(string.Empty, label.ThrowIn);
        Assert.Equal(string.Empty, label.Penalty);
        Assert.False(label.IsSetDefaultValue);
    }

    [Fact]
    public void NextFiveMinutes_NullOrWhitespaceOdds_UsesDefault()
    {
        Assert.True(ResultItemLabel.Create(FastMarketBetType.NextFiveMinutes, null).IsSetDefaultValue);
        Assert.True(ResultItemLabel.Create(FastMarketBetType.NextFiveMinutes, "   ").IsSetDefaultValue);
    }
}
