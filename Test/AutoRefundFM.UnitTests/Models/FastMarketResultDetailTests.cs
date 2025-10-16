using AutoRefundFM.Models;
using Xunit;

namespace AutoRefundFM.UnitTests.Models;

public class FastMarketResultDetailTests
{
    [Theory]
    [InlineData(FastMarketBetType.NextOneMinute, 1, "01:00-01:59")]
    [InlineData(FastMarketBetType.NextOneMinute, 10, "10:00-10:59")]
    [InlineData(FastMarketBetType.TimeMachineMinutes, 0, "00:00-01:00")]
    [InlineData(FastMarketBetType.TimeMachineMinutes, 9, "09:01-10:00")]
    [InlineData(FastMarketBetType.TimeMachineMinutes, 10, "10:01-11:00")]
    [InlineData(FastMarketBetType.TimeMachineMinutes, 45, "First Half Injury Time")]
    [InlineData(FastMarketBetType.NextFiveMinutes, 1, "01:00-05:59")]
    public void PositionName_ShouldReturnExpected(int betType, int position, string expected)
    {
        var detail = new FastMarketResultDetail { BetType = betType, Position = position };
        Assert.Equal(expected, detail.PositionName);
    }

    [Fact]
    public void ResultItem_WhenCheckedIsTrue_ShouldReturnCachedValue()
    {
        var detail = new FastMarketResultDetail
        {
            Checked = true,
            ResultItem = new ResultItem { Goal = true, Corner = true }
        };
        Assert.True(detail.ResultItem.Goal);
        Assert.True(detail.ResultItem.Corner);
    }

    [Fact]
    public void ResultItem_WhenResultIsSet_ShouldMapCorrectly()
    {
        var detail = new FastMarketResultDetail { BetType = FastMarketBetType.NextOneMinute, Result = 3 };
        var result = detail.ResultItem;
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(FastMarketBetType.TimeMachineHdp, 23, 3, 2)]
    [InlineData(FastMarketBetType.TimeMachineHdp, 45, 5, 4)]
    [InlineData(FastMarketBetType.NextOneMinute, 23, 0, 0)]
    public void HomeGoal_AwayGoal_ShouldReturnExpected(int betType, int result, int expectedHome, int expectedAway)
    {
        var detail = new FastMarketResultDetail { BetType = betType, Result = result };
        Assert.Equal(expectedHome, detail.HomeGoal);
        Assert.Equal(expectedAway, detail.AwayGoal);
    }

    [Theory]
    [InlineData(3, "Running")]
    [InlineData(4, "Completed (Auto)")]
    [InlineData(5, "Completed (Manual)")]
    [InlineData(6, "Refund")]
    [InlineData(7, "Refund SDT")]
    public void ResultStatusName_ShouldReturnExpected(int status, string expected)
    {
        var detail = new FastMarketResultDetail { ResultStatus = status };
        Assert.Equal(expected, detail.ResultStatusName);
    }

    [Theory]
    [InlineData(2, "Running")]
    [InlineData(3, "Calculated")]
    [InlineData(4, "No Ticket")]
    [InlineData(5, "Marked")]
    public void TicketStatusName_ShouldReturnExpected(int status, string expected)
    {
        var detail = new FastMarketResultDetail { TicketStatus = status };
        Assert.Equal(expected, detail.TicketStatusName);
    }

    [Fact]
    public void Equals_And_GetHashCode_ShouldWork()
    {
        var a = new FastMarketResultDetail { BetType = 1, Position = 2 };
        var b = new FastMarketResultDetail { BetType = 1, Position = 2 };
        var c = new FastMarketResultDetail { BetType = 2, Position = 2 };
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
