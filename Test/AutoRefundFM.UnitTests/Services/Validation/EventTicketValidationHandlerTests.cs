using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class EventTicketValidationHandlerTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsResultFromScaleOutSettleService()
    {
        // Arrange
        var scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        var expectedErrorCode = 123;
        var expectedErrorMsg = "Some error";
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage
            {
                ErrorCode = expectedErrorCode,
                ErrorMessage = expectedErrorMsg
            });
        var handler = new EventTicketValidationHandler(scaleOutSettleService);
        var context = new ValidationContext
        {
            Session = new SpecialSession { MatchId = 99 },
            BetTypeRootXml = "<bettypes/>",
            EventCountersXml = "<counters/>"
        };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(expectedErrorCode, result.ErrCode);
        Assert.Equal(expectedErrorMsg, result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_SetsIsAutoToFalseOnRequest()
    {
        // Arrange
        var scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        EventTicketFastMarketRequest? capturedRequest = null;
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Do<EventTicketFastMarketRequest>(req => capturedRequest = req))
            .Returns(new ResultMessage());
        var handler = new EventTicketValidationHandler(scaleOutSettleService);
        var context = new ValidationContext
        {
            Session = new SpecialSession { MatchId = 1 },
            BetTypeRootXml = "<bettypes/>",
            EventCountersXml = "<counters/>"
        };

        // Act
        await handler.Handle(context);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.False(capturedRequest!.IsAuto);
        Assert.Equal(1, capturedRequest.MatchId);
        Assert.Equal("<bettypes/>", capturedRequest.Results);
        //Assert.Equal("<counters/>", capturedRequest.EventCounters);
    }
}
