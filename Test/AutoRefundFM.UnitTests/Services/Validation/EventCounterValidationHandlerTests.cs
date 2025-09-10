using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class EventCounterValidationHandlerTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_WhenAllEventCountersAreValid()
    {
        // Arrange
        var handler = new EventCounterValidationHandler();
        var eventResultItem = new EventResultItem { Goal = "1", Corner = "1", FreeKick = "1", GoalKick = "1", ThrowIn = "1" };
        var eventCounter = new EventCounter { EventResultItem = eventResultItem };
        var resultItemLabel = new ResultItemLabel { Goal = "0.5", Corner = "0.5", FreeKick = "0.5", GoalKick = "0.5", ThrowIn = "0.5" };
        var resultItem = new ResultItem { Goal = true, Corner = true, FreeKick = true, GoalKick = true, ThrowIn = true };
        var fastMarketResult = new FastMarketResultDetail
        {
            BetType = FastMarketBetType.NextOneMinute,
            EventCounters = new List<EventCounter> { eventCounter },
            ResultItem = resultItem,
            ResultItemLabel = resultItemLabel
        };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail> { fastMarketResult }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }


    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_WhenBetTypeIsUnknown()
    {
        // Arrange
        var handler = new EventCounterValidationHandler();
        var fastMarketResult = new FastMarketResultDetail
        {
            BetType = 999,
            EventCounters = new List<EventCounter>(),
            ResultItem = new ResultItem(),
            ResultItemLabel = new ResultItemLabel()
        };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail> { fastMarketResult }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenEventCountersIsEmpty_ForNextOneMinute()
    {
        // Arrange
        var handler = new EventCounterValidationHandler();
        var fastMarketResult = new FastMarketResultDetail
        {
            BetType = FastMarketBetType.NextOneMinute,
            EventCounters = new List<EventCounter>(),
            ResultItem = new ResultItem(),
            ResultItemLabel = new ResultItemLabel()
        };
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail> { fastMarketResult }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoEventCounters, result.ErrCode);
    }
}
