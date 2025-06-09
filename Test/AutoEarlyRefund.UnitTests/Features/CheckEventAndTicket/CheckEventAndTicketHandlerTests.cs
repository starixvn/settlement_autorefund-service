using AutoEarlyRefund.Features.CheckEventAndTicket;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.CheckEventAndTicket;

public class CheckEventAndTicketHandlerTests : TestBase
{
    private readonly GeneralService generalService;
    private readonly IDynamicRepository repository;
    private readonly XmlConvertService xmlConvertService;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly IValidateHandler nextHandler;
    private readonly CheckEventAndTicketHandler handler;

    public CheckEventAndTicketHandlerTests()
    {
        generalService = Substitute.For<GeneralService>();
        repository = Substitute.For<IDynamicRepository>();
        xmlConvertService = Substitute.For<XmlConvertService>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        nextHandler = Substitute.For<IValidateHandler>();

        appSettingsKey.Value.Returns(new AppSettingsKey
        {
            RetryCount = 3,
            RetryInterval = 1000,
            SiteDbServer = ["Server1", "Server2"]
        });

        handler = new CheckEventAndTicketHandler(
            generalService,
            repository,
            xmlConvertService,
            appSettingsKey,
            nextHandler);
    }

    [Fact]
    public async Task HandleAsync_WhenEventValidationFails_ShouldReturnError()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var eventResult = new ResultMessage { ErrorCode = 1001, ErrorMessage = "Event not found" };
        generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(eventResult);

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(1001, result.ErrCode);
        Assert.Equal("Event not found", result.ErrMsg);
        await nextHandler.DidNotReceive().HandleAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task HandleAsync_WhenTicketValidationSucceeds_ShouldCallNextHandler()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var eventResult = new ResultMessage { ErrorCode = 0 };
        generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(eventResult);

        await repository.ExecuteAsync(Arg.Any<ValidateRefundTicketCriteria>());

        var nextResult = new Result { ErrCode = 0 };
        nextHandler.HandleAsync(Arg.Any<Match>()).Returns(nextResult);

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(0, result.ErrCode);
        await nextHandler.Received(1).HandleAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task HandleAsync_WhenTicketValidationFails_ShouldNotCallNextHandler()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var eventResult = new ResultMessage { ErrorCode = 0 };
        generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(eventResult);

        await repository.ExecuteAsync(Arg.Do<ValidateRefundTicketCriteria>(c =>
        {
            c.ErrCode = 1002;
            c.ErrMsg = "Error on Server1";
        }));

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(1002, result.ErrCode);
        await nextHandler.DidNotReceive().HandleAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task HandleAsync_WhenNextHandlerIsNull_ShouldReturnCurrentResult()
    {
        // Arrange
        var ticketHandler = new CheckEventAndTicketHandler(
            generalService,
            repository,
            xmlConvertService,
            appSettingsKey,
            next: null);

        var match = new Match { MatchId = 123 };
        var eventResult = new ResultMessage { ErrorCode = 0 };
        generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(eventResult);

        await repository.ExecuteAsync(Arg.Any<ValidateRefundTicketCriteria>());

        // Act
        var result = await ticketHandler.HandleAsync(match);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task HandleAsync_WhenAllServersFail_ShouldReturnFirstError()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var eventResult = new ResultMessage { ErrorCode = 0 };
        generalService.ExecuteSingle(Arg.Any<ValidateEventCriteria>()).Returns(eventResult);

        await repository.ExecuteAsync(Arg.Do<ValidateRefundTicketCriteria>(c =>
        {
            c.ErrCode = 1003;
            c.ErrMsg = "Error on Server1";
        }));

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(1003, result.ErrCode);
        Assert.Equal("Error on Server1", result.ErrMsg);
    }
}