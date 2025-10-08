using AutoRefundFM.Features.CheckMarkedTicket;
using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class MarkedTicketsValidationHandlerTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenMarkedTicketsFound()
    {
        // Arrange
        var checkMarkedTicketHandler = Substitute.For<ICheckMarkedTicketHandler>();
        checkMarkedTicketHandler.Handle(Arg.Any<int>(), Arg.Any<string>()).Returns(true);
        var handler = new MarkedTicketsValidationHandler(checkMarkedTicketHandler);
        var context = new ValidationContext
        {
            Session = new SpecialSession { MatchId = 123 },
            BetTypeRootXml = "<bettypes/>"
        };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.MarkedTicketsFound, result.ErrCode);
        Assert.Contains("marked tickets exist", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_WhenNoMarkedTicketsFound()
    {
        // Arrange
        var checkMarkedTicketHandler = Substitute.For<ICheckMarkedTicketHandler>();
        checkMarkedTicketHandler.Handle(Arg.Any<int>(), Arg.Any<string>()).Returns(false);
        var handler = new MarkedTicketsValidationHandler(checkMarkedTicketHandler);
        var context = new ValidationContext
        {
            Session = new SpecialSession { MatchId = 456 },
            BetTypeRootXml = "<bettypes/>"
        };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }
}
