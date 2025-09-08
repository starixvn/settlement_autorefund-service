using AutoRefundFM.Models;
using AutoRefundFM.Services;

namespace AutoRefundFM.UnitTests.Services;

public class EventCounterServiceTests
{
    private readonly EventCounterService sut = new();

    private EventCounter CreateEventCounter(int position, string goal = "1", string corner = "2", string freeKick = "3", string goalKick = "4", string throwIn = "5", string penalty = "6")
    {
        return new EventCounter
        {
            Position = position,
            EventResultItem = new EventResultItem
            {
                Goal = goal,
                Corner = corner,
                FreeKick = freeKick,
                GoalKick = goalKick,
                ThrowIn = throwIn,
                Penalty = penalty
            }
        };
    }

    private ResultItemLabel CreateLabel(string goal = "1.5", string corner = "2.5", string freeKick = "3.5", string goalKick = "4.5", string throwIn = "5.5", string penalty = "6.5")
    {
        return new ResultItemLabel
        {
            Goal = goal,
            Corner = corner,
            FreeKick = freeKick,
            GoalKick = goalKick,
            ThrowIn = throwIn,
            Penalty = penalty
        };
    }

    private ResultItem CreateResultItem(bool goal = true, bool corner = false, bool freeKick = false, bool goalKick = false, bool throwIn = false, bool penalty = false)
    {
        return new ResultItem
        {
            Goal = goal,
            Corner = corner,
            FreeKick = freeKick,
            GoalKick = goalKick,
            ThrowIn = throwIn,
            Penalty = penalty
        };
    }

    [Fact]
    public void GetEventCountersForBetType_NextOneMinute_ReturnsSingleMatching()
    {
        // Arrange
        var counters = new List<EventCounter>
        {
            CreateEventCounter(10),
            CreateEventCounter(20)
        };
        var item = new FastMarketResultDetail { BetType = FastMarketBetType.NextOneMinute, Position = 20 };

        // Act
        var result = sut.GetEventCountersForBetType(item, counters);

        // Assert
        Assert.Single(result);
        Assert.Equal(20, result[0].Position);
    }

    [Fact]
    public void GetEventCountersForBetType_NextFiveMinutes_ReturnsRange()
    {
        // Arrange
        var counters = new List<EventCounter>
        {
            CreateEventCounter(10),
            CreateEventCounter(11),
            CreateEventCounter(12),
            CreateEventCounter(13),
            CreateEventCounter(14),
            CreateEventCounter(15)
        };
        var item = new FastMarketResultDetail { BetType = FastMarketBetType.NextFiveMinutes, Position = 12 };

        // Act
        var result = sut.GetEventCountersForBetType(item, counters);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(result, c => c.Position == 12);
        Assert.Contains(result, c => c.Position == 13);
        Assert.Contains(result, c => c.Position == 14);
        Assert.Contains(result, c => c.Position == 15);
    }

    [Fact]
    public void GetEventCountersForBetType_UnknownType_ReturnsEmpty()
    {
        // Arrange
        var counters = new List<EventCounter> { CreateEventCounter(1) };
        var item = new FastMarketResultDetail { BetType = 999, Position = 1 };

        // Act
        var result = sut.GetEventCountersForBetType(item, counters);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateEventCounterWithBetResult_NoEventCounters_ReturnsFalse()
    {
        // Arrange
        var item = new FastMarketResultDetail
        {
            BetType = FastMarketBetType.NextOneMinute,
            EventCounters = new List<EventCounter>()
        };

        // Act
        var result = sut.ValidateEventCounterWithBetResult(item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateEventCounterWithBetResult_NextOneMinute_Valid()
    {
        // Arrange
        var eventCounter = CreateEventCounter(1, goal: "1");
        var item = new FastMarketResultDetail
        {
            BetType = FastMarketBetType.NextOneMinute,
            EventCounters = new List<EventCounter> { eventCounter },
            ResultItem = CreateResultItem(goal: true),
        };

        // Act
        var result = sut.ValidateEventCounterWithBetResult(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateEventCounterWithBetResult_NextFiveMinutes_Valid()
    {
        // Arrange
        var eventCounters = new List<EventCounter>
        {
            CreateEventCounter(1, goal: "2"),
            CreateEventCounter(2, goal: "2")
        };
        var item = new FastMarketResultDetail
        {
            BetType = FastMarketBetType.NextFiveMinutes,
            EventCounters = eventCounters,
            ResultItem = CreateResultItem(goal: true),
            ResultItemLabel = CreateLabel(goal: "3")
        };

        // Act
        var result = sut.ValidateEventCounterWithBetResult(item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateEventCounterWithBetResult_UnknownType_ReturnsTrue()
    {
        // Arrange
        var eventCounter = CreateEventCounter(1, goal: "1");
        var item = new FastMarketResultDetail
        {
            BetType = 999,
            EventCounters = new List<EventCounter> { eventCounter }
        };

        // Act
        var result = sut.ValidateEventCounterWithBetResult(item);

        // Assert
        Assert.True(result);
    }
}
