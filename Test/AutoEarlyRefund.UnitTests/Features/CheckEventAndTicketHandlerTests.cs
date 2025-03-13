using AutoEarlyRefund.Features.CheckEventAndTicket;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using AutoRefund.Core.Models;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features;

public class CheckEventAndTicketHandlerTests
{
    private readonly IValidateHandler _nextHandler;
    private readonly GeneralService _generalService;
    private readonly XmlConvertService _xmlConvertService;
    private readonly CheckEventAndTicketHandler _handler;

    public CheckEventAndTicketHandlerTests()
    {
        _nextHandler = Substitute.For<IValidateHandler>();
        _generalService = Substitute.For<GeneralService>();
        _xmlConvertService = Substitute.For<XmlConvertService>();
        _handler = new CheckEventAndTicketHandler(
            _generalService,
            _xmlConvertService,
            _nextHandler);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnErrorResult_WhenEventResultHasError()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var newMatch = match.WithResourcesXml(_xmlConvertService, false);
        var eventResult = new ResultMessage { ErrorCode = 1, ErrorMessage = "Error" };
        _generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(eventResult);

        // Act
        var result = await _handler.HandleAsync(match);

        // Assert
        Assert.Equal(1, result.ErrCode);
        Assert.Equal("Error", result.ErrMsg);
    }


    [Fact]
    public async Task HandleAsync_ShouldCallNextHandler_WhenNoErrorAndNextHandlerIsNotNull()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var newMatch = match.WithResourcesXml(_xmlConvertService, false);
        var eventResult = new Result { ErrCode = 0 };
        var resultMsg = new ResultMessage { ErrorCode = 0, ErrorMessage = "Success" };
        _generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(resultMsg);
        _generalService.ExecuteAsync(Arg.Any<ValidateRefundTicketCriteria>()).Returns(resultMsg);
        _nextHandler.HandleAsync(Arg.Any<Match>()).Returns(eventResult);

        // Act
        var result = await _handler.HandleAsync(newMatch);

        // Assert
        Assert.Equal(0, result.ErrCode);
        await _nextHandler.Received(1).HandleAsync(Arg.Any<Match>());
    }
}
