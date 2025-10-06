using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class EventCounterValidationHandlerTests
{
    private readonly EventCounterValidationHandler sut = new(null);

    private static IOptions<AppSettingsKey> CreateOptions(int retryProcessScore = 60000)
    {
        var options = Options.Create(new AppSettingsKey { RetryProcessScore = retryProcessScore });
        return options;
    }

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

    //[Fact]
    //public async Task ProcessAsync_WithNoEventCounters_ReturnsError()
    //{
    //    // Arrange
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter>())
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

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

    //[Fact]
    //public async Task ProcessAsync_WithMixedValidAndInvalidResults_ReturnsError()
    //{
    //    // Arrange
    //    var validResult = CreateResultDetail(FastMarketBetType.NextOneMinute,
    //        new List<EventCounter> { CreateEventCounter(goal: "1") },
    //        new ResultItem { Goal = true });

    //    var invalidResult = CreateResultDetail(FastMarketBetType.NextOneMinute,
    //        new List<EventCounter>(),
    //        new ResultItem { Goal = true });

    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail> { validResult, invalidResult }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    #endregion

    #region NextOneMinute Validation Tests

    //[Fact]
    //public async Task ProcessAsync_NextOneMinute_WithEmptyEventCounters_ReturnsError()
    //{
    //    // Arrange
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter>())
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //}

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

    #region Time Expiration Tests

    [Fact]
    public async Task ProcessAsync_WithExpiredSession_ReturnsMarkFailTrue()
    {
        // Arrange
        var options = CreateOptions(retryProcessScore: 1000); // 1 second
        var sutWithOptions = new EventCounterValidationHandler(options);

        var session = new SpecialSession
        {
            FirstProcess = DateTimeOffset.Now.AddSeconds(-2), // 2 seconds ago
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter>())
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sutWithOptions.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
        Assert.True(result.MarkFail);
        Assert.Contains("mismatch", result.ErrMsg);
    }

    //[Fact]
    //public async Task ProcessAsync_WithNonExpiredSession_ReturnsMarkFailFalse()
    //{
    //    // Arrange
    //    var options = CreateOptions(retryProcessScore: 30000); // 30 seconds
    //    var sutWithOptions = new EventCounterValidationHandler(options);

    //    var session = new SpecialSession
    //    {
    //        FirstProcess = DateTimeOffset.Now.AddSeconds(-10), // 10 seconds ago
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter>())
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sutWithOptions.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.False(result.MarkFail);
    //}

    #endregion

    #region Integer Parsing Error Tests

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithInvalidIntegerInEventCounter_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "invalid", corner: "2", freeKick: "1", goalKick: "0", throwIn: "2", penalty: "1"),
    //        CreateEventCounter(goal: "0", corner: "1", freeKick: "0", goalKick: "1", throwIn: "1", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = true, GoalKick = true, ThrowIn = true, Penalty = true };
    //    var resultItemLabel = new ResultItemLabel { Goal = "1.5", Corner = "2.5", FreeKick = "1.5", GoalKick = "1.5", ThrowIn = "2.5", Penalty = "1.5" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithEmptyStringInEventCounter_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "", corner: "2", freeKick: "1", goalKick: "0", throwIn: "2", penalty: "1"),
    //        CreateEventCounter(goal: "0", corner: "1", freeKick: "0", goalKick: "1", throwIn: "1", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = true, GoalKick = true, ThrowIn = true, Penalty = true };
    //    var resultItemLabel = new ResultItemLabel { Goal = "1.5", Corner = "2.5", FreeKick = "1.5", GoalKick = "1.5", ThrowIn = "2.5", Penalty = "1.5" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithNullStringInEventCounter_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: null!, corner: "2", freeKick: "1", goalKick: "0", throwIn: "2", penalty: "1"),
    //        CreateEventCounter(goal: "0", corner: "1", freeKick: "0", goalKick: "1", throwIn: "1", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = true, GoalKick = true, ThrowIn = true, Penalty = true };
    //    var resultItemLabel = new ResultItemLabel { Goal = "1.5", Corner = "2.5", FreeKick = "1.5", GoalKick = "1.5", ThrowIn = "2.5", Penalty = "1.5" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    #endregion

    #region Threshold Validation Tests

    [Fact]
    public async Task ProcessAsync_NextOneMinute_WithEventValueAboveThresholdAndBetTrue_ReturnsSuccess()
    {
        // Arrange
        var eventCounter = CreateEventCounter(goal: "0.6"); // Above 0.5 threshold
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
    public async Task ProcessAsync_NextOneMinute_WithEventValueBelowThresholdAndBetFalse_ReturnsSuccess()
    {
        // Arrange
        var eventCounter = CreateEventCounter(goal: "0.4"); // Below 0.5 threshold
        var resultItem = new ResultItem { Goal = false };
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

    //[Fact]
    //public async Task ProcessAsync_NextOneMinute_WithEventValueAboveThresholdAndBetFalse_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounter = CreateEventCounter(goal: "0.6"); // Above 0.5 threshold
    //    var resultItem = new ResultItem { Goal = false }; // But bet is false
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter> { eventCounter }, resultItem)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextOneMinute_WithEventValueBelowThresholdAndBetTrue_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounter = CreateEventCounter(goal: "0.4"); // Below 0.5 threshold
    //    var resultItem = new ResultItem { Goal = true }; // But bet is true
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter> { eventCounter }, resultItem)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextOneMinute_WithInvalidEventValue_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounter = CreateEventCounter(goal: "invalid");
    //    var resultItem = new ResultItem { Goal = true };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextOneMinute, new List<EventCounter> { eventCounter }, resultItem)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    #endregion

    #region Five Minutes Validation Edge Cases

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithTotalEventsAboveExpectedAndBetTrue_ReturnsSuccess()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "2", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0"),
    //        CreateEventCounter(goal: "1", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = true, Corner = false, FreeKick = false, GoalKick = false, ThrowIn = false, Penalty = false };
    //    var resultItemLabel = new ResultItemLabel { Goal = "2.5", Corner = "0", FreeKick = "0", GoalKick = "0", ThrowIn = "0", Penalty = "0" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(0, result.ErrCode);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithTotalEventsBelowExpectedAndBetFalse_ReturnsSuccess()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "1", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0"),
    //        CreateEventCounter(goal: "0", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = false, Corner = false, FreeKick = false, GoalKick = false, ThrowIn = false, Penalty = false };
    //    var resultItemLabel = new ResultItemLabel { Goal = "2.5", Corner = "0", FreeKick = "0", GoalKick = "0", ThrowIn = "0", Penalty = "0" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(0, result.ErrCode);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithTotalEventsAboveExpectedAndBetFalse_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "3", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0"),
    //        CreateEventCounter(goal: "1", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = false, Corner = false, FreeKick = false, GoalKick = false, ThrowIn = false, Penalty = false };
    //    var resultItemLabel = new ResultItemLabel { Goal = "2.5", Corner = "0", FreeKick = "0", GoalKick = "0", ThrowIn = "0", Penalty = "0" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithTotalEventsBelowExpectedAndBetTrue_ReturnsError()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "1", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0"),
    //        CreateEventCounter(goal: "0", corner: "0", freeKick: "0", goalKick: "0", throwIn: "0", penalty: "0")
    //    };
    //    var resultItem = new ResultItem { Goal = true, Corner = false, FreeKick = false, GoalKick = false, ThrowIn = false, Penalty = false };
    //    var resultItemLabel = new ResultItemLabel { Goal = "2.5", Corner = "0", FreeKick = "0", GoalKick = "0", ThrowIn = "0", Penalty = "0" };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, resultItemLabel)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    //    Assert.Contains("not consistent", result.ErrMsg);
    //}

    //#endregion

    //#region Additional Edge Cases

    //[Fact]
    //public async Task ProcessAsync_NextFiveMinutes_WithNullResultItemLabel_HandlesGracefully()
    //{
    //    // Arrange
    //    var eventCounters = new List<EventCounter>
    //    {
    //        CreateEventCounter(goal: "1", corner: "2", freeKick: "1", goalKick: "0", throwIn: "2", penalty: "1")
    //    };
    //    var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = true, GoalKick = false, ThrowIn = true, Penalty = true };
    //    var session = new SpecialSession
    //    {
    //        FastMarketResultDetails = new List<FastMarketResultDetail>
    //        {
    //            CreateResultDetail(FastMarketBetType.NextFiveMinutes, eventCounters, resultItem, null)
    //        }
    //    };
    //    var context = new ValidationContext { Session = session };

    //    // Act
    //    var result = await sut.Handle(context);

    //    // Assert
    //    Assert.Equal(0, result.ErrCode);
    //}

    [Fact]
    public async Task ProcessAsync_WithAllEventTypes_ValidatesCorrectly()
    {
        // Arrange - Test all event types for NextOneMinute
        var eventCounter = CreateEventCounter(
            goal: "0.6",
            corner: "0.4",
            freeKick: "0.8",
            goalKick: "0.2",
            throwIn: "0.9",
            penalty: "0.7"
        );
        var resultItem = new ResultItem
        {
            Goal = true,    // 0.6 > 0.5 ✓
            Corner = false, // 0.4 < 0.5 ✓
            FreeKick = true, // 0.8 > 0.5 ✓
            GoalKick = false, // 0.2 < 0.5 ✓
            ThrowIn = true   // 0.9 > 0.5 ✓
        };
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
}
