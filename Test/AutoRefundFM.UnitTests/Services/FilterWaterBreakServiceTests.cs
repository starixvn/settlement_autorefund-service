using AutoRefundFM.Models;
using AutoRefundFM.Services;

namespace AutoRefundFM.UnitTests.Services;

public class FilterWaterBreakServiceTests
{
    private readonly FilterWaterBreakService sut = new();

    private SpecialSession CreateSession(int start, int end)
        => new SpecialSession { StartMinute = start, EndMinute = end };

    private FastMarketResultDetail CreateResult(int betType, int position)
        => new FastMarketResultDetail { BetType = betType, Position = position };

    [Fact]
    public void ApplyFilterByWaterBreak_TimeMachineTypes_ReturnsEmpty()
    {
        // Arrange
        var session = CreateSession(10, 15);
        var results = new List<FastMarketResultDetail>
        {
            CreateResult(FastMarketBetType.TimeMachineHdp, 12),
            CreateResult(FastMarketBetType.TimeMachineMinutes, 13)
        };

        // Act
        var filtered = sut.ApplyFilterByWaterBreak(results, session);

        // Assert
        Assert.Empty(filtered);
    }

    [Fact]
    public void ApplyFilterByWaterBreak_OneMinuteTypes_SingleMinute()
    {
        // Arrange
        var session = CreateSession(10, 10);
        var results = new List<FastMarketResultDetail>
        {
            CreateResult(FastMarketBetType.NextOneMinute, 10),
            CreateResult(FastMarketBetType.NextOneMinute, 11),
            CreateResult(FastMarketBetType.NextOneMinuteSetPiece, 10),
            CreateResult(FastMarketBetType.NextOneMinuteSetPiece, 9)
        };

        // Act
        var filtered = sut.ApplyFilterByWaterBreak(results, session);

        // Assert
        Assert.All(filtered, r => Assert.Equal(10, r.Position));
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void ApplyFilterByWaterBreak_OneMinuteTypes_MultiMinute()
    {
        // Arrange
        var session = CreateSession(10, 12);
        var results = new List<FastMarketResultDetail>
        {
            CreateResult(FastMarketBetType.NextOneMinute, 10),
            CreateResult(FastMarketBetType.NextOneMinute, 11),
            CreateResult(FastMarketBetType.NextOneMinute, 13),
            CreateResult(FastMarketBetType.CombinationNextOneMinute, 12)
        };

        // Act
        var filtered = sut.ApplyFilterByWaterBreak(results, session);

        // Assert
        Assert.Equal(3, filtered.Count);
        Assert.Contains(filtered, r => r.Position == 10);
        Assert.Contains(filtered, r => r.Position == 11);
        Assert.Contains(filtered, r => r.Position == 12);
    }

    [Fact]
    public void ApplyFilterByWaterBreak_FiveMinuteTypes()
    {
        // Arrange
        var session = CreateSession(10, 15);
        var results = new List<FastMarketResultDetail>
        {
            CreateResult(99, 6), // outside
            CreateResult(99, 8), // inside
            CreateResult(99, 15) // inside
        };

        // Act
        var filtered = sut.ApplyFilterByWaterBreak(results, session);

        // Assert
        Assert.Equal(3, filtered.Count);
        Assert.Contains(filtered, r => r.Position == 6);
        Assert.Contains(filtered, r => r.Position == 8);
        Assert.Contains(filtered, r => r.Position == 15);
    }

    [Fact]
    public void ApplyFilterEventCountWaterBreak_FiltersCorrectly()
    {
        // Arrange
        var session = CreateSession(10, 15);
        var counters = new List<EventCounter>
        {
            new EventCounter { Position = 5 },
            new EventCounter { Position = 8 },
            new EventCounter { Position = 15 },
            new EventCounter { Position = 21 }
        };

        // Act
        var filtered = sut.ApplyFilterEventCountWaterBreak(counters, session);

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, c => c.Position == 8);
        Assert.Contains(filtered, c => c.Position == 15);
    }
}
