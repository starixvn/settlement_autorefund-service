using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class EventCounterValidationHandlerTests
{
    private readonly EventCounterValidationHandler sut = new(null);

    private static FastMarketResultDetail CreateResultDetail(
        int betType,
        List<EventCounter> eventCounters,
        ResultItem? resultItem = null,
        ResultItemLabel? resultItemLabel = null)
    {
        return new FastMarketResultDetail
        {
            BetType = betType,
            EventCounters = eventCounters,
            ResultItem = resultItem ?? new ResultItem(),
            ResultItemLabel = resultItemLabel ?? new ResultItemLabel()
        };
    }

    private static EventCounter CreateEventCounter(string goal = "0", string corner = "0", string freeKick = "0",
        string goalKick = "0", string throwIn = "0", string penalty = "0")
    {
        return new EventCounter
        {
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

    #region ProcessAsync Tests

    [Fact]
    public async Task ProcessAsync_WithNoEventCounters_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter>())
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
        Assert.Contains("not consistent", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_WithValidNextOneMinuteEvents_ReturnsSuccess()
    {
        // Arrange
        var eventCounter = CreateEventCounter(goal: "1");
        var resultItem = new ResultItem { Goal = true };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter> { eventCounter }, resultItem)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }


    [Fact]
    public async Task ProcessAsync_WithUnknownBetType_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(999, new List<EventCounter>())
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_WithMixedValidAndInvalidResults_ReturnsError()
    {
        // Arrange
        var validResult = CreateResultDetail(FastMarketBetType.NextOneMinute,
            new List<EventCounter> { CreateEventCounter(goal: "1") },
            new ResultItem { Goal = true });

        var invalidResult = CreateResultDetail(FastMarketBetType.NextOneMinute,
            new List<EventCounter>(),
            new ResultItem { Goal = true });

        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail> { validResult, invalidResult }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
        Assert.Contains("not consistent", result.ErrMsg);
    }

    #endregion

    #region NextOneMinute Validation Tests

    [Fact]
    public async Task ProcessAsync_NextOneMinute_WithEmptyEventCounters_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter>())
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_NextOneMinute_WithAllValidEvents_ReturnsSuccess()
    {
        // Arrange
        var eventCounter = CreateEventCounter(goal: "1", corner: "0.8", freeKick: "0.3", goalKick: "0.2", throwIn: "0.9");
        var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = false, GoalKick = false, ThrowIn = true };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter> { eventCounter }, resultItem)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    #endregion

    #region NextFiveMinutes Validation Tests

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithAllValidEvents_ReturnsSuccess()
    {
        // Arrange
        var eventCounters = new List<EventCounter>
        {
            CreateEventCounter(goal: "1", corner: "2", freeKick: "1", goalKick: "0", throwIn: "2", penalty: "1"),
            CreateEventCounter(goal: "0", corner: "1", freeKick: "0", goalKick: "1", throwIn: "1", penalty: "0")
        };
        var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = true, GoalKick = true, ThrowIn = true, Penalty = true };
        var resultItemLabel = new ResultItemLabel { Goal = "1.5", Corner = "2.5", FreeKick = "1.5", GoalKick = "1.5", ThrowIn = "2.5", Penalty = "1.5" };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task ProcessAsync_WithNullSession_ThrowsException()
    {
        // Arrange
        var context = new ValidationContext { Session = null! };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => sut.Handle(context));
    }

    [Fact]
    public async Task ProcessAsync_WithNullFastMarketResultDetails_ThrowsException()
    {
        // Arrange
        var session = new SpecialSession { FastMarketResultDetails = null! };
        var context = new ValidationContext { Session = session };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => sut.Handle(context));
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyFastMarketResultDetails_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession { FastMarketResultDetails = new List<FastMarketResultDetail>() };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }



    [Fact]
    public async Task ProcessAsync_WithNullResultItem_HandlesGracefully()
    {
        // Arrange
        var eventCounters = new List<EventCounter> { CreateEventCounter(goal: "1") };
        var resultDetail = new FastMarketResultDetail
        {
            BetType = FastMarketBetType.NextOneMinute,
            EventCounters = eventCounters,
            ResultItem = null!,
            ResultItemLabel = new ResultItemLabel()
        };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail> { resultDetail }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_WithNullEventResultItem_HandlesGracefully()
    {
        // Arrange
        var eventCounter = new EventCounter { EventResultItem = null! };
        var resultItem = new ResultItem { Goal = true };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter> { eventCounter }, resultItem)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    #endregion
}
