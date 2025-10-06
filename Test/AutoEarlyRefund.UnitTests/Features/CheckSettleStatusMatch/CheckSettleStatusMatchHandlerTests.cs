using AutoEarlyRefund.Features.CheckSettleStatusMatch;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AutoEarlyRefund.UnitTests.Features.CheckSettleStatusMatch;

public class CheckSettleStatusMatchHandlerTests : TestBase
{
    private readonly IValidateHandler nextHandler;
    private readonly IDynamicRepository repository;
    private readonly CheckSettleStatusMatchHandler handler;

    public CheckSettleStatusMatchHandlerTests()
    {
        nextHandler = Substitute.For<IValidateHandler>();
        repository = Substitute.For<IDynamicRepository>();
        handler = new CheckSettleStatusMatchHandler(nextHandler, repository);
    }

    [Fact]
    public async Task HandleAsync_WhenErrCodeIsZero_ShouldCallNextHandler()
    {
        // Arrange
        var match = CreateSampleMatch();
        var expectedResult = new Result { ErrCode = 0, ErrMsg = "Success" };
        
        // Setup repository to modify criteria with success result
        await repository.ExecuteAsync(Arg.Do<CheckSettleStatusMatchCriteria>(c =>
        {
            c.ErrCode = 0;
            c.ErrMsg = "Success";
        }));

        nextHandler.HandleAsync(match).Returns(expectedResult);

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.Equal("Success", result.ErrMsg);
        await nextHandler.Received(1).HandleAsync(match);
    }

    [Fact]
    public async Task HandleAsync_WhenErrCodeIsNotZero_ShouldNotCallNextHandler()
    {
        // Arrange
        var match = CreateSampleMatch();
        
        // Setup repository to return error (ErrCode != 0)
        await repository.ExecuteAsync(Arg.Do<CheckSettleStatusMatchCriteria>(c =>
        {
            c.ErrCode = 1;
            c.ErrMsg = "Error occurred";
        }));

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(1, result.ErrCode);
        Assert.Equal("Error occurred", result.ErrMsg);
        await nextHandler.DidNotReceive().HandleAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task HandleAsync_WhenNextHandlerIsNull_ShouldReturnResultWithoutCallingNext()
    {
        // Arrange
        var handlerWithNullNext = new CheckSettleStatusMatchHandler(null, repository);
        var match = CreateSampleMatch();
        
        // Setup repository to return success
        await repository.ExecuteAsync(Arg.Do<CheckSettleStatusMatchCriteria>(c =>
        {
            c.ErrCode = 0;
            c.ErrMsg = "Success";
        }));

        // Act
        var result = await handlerWithNullNext.HandleAsync(match);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.Equal("Success", result.ErrMsg);
        await nextHandler.DidNotReceive().HandleAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateCorrectCriteria()
    {
        // Arrange
        var match = CreateSampleMatch();
        CheckSettleStatusMatchCriteria? capturedCriteria = null;
        
        await repository.ExecuteAsync(Arg.Do<CheckSettleStatusMatchCriteria>(c =>
        {
            capturedCriteria = c;
            c.ErrCode = 0;
            c.ErrMsg = "Success";
        }));

        // Act
        await handler.HandleAsync(match);

        // Assert
        Assert.NotNull(capturedCriteria);
        Assert.Equal(match.MatchId, capturedCriteria.MatchId);
        Assert.Equal("REFUND", capturedCriteria.Action);
        Assert.True(capturedCriteria.IsEarly);
        Assert.True(capturedCriteria.IsAutoRefund);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var match = CreateSampleMatch();
        var expectedException = new InvalidOperationException("Database error");
        
        repository.ExecuteAsync(Arg.Any<CheckSettleStatusMatchCriteria>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(match));
        
        Assert.Equal("Database error", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenNextHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var match = CreateSampleMatch();
        var expectedException = new InvalidOperationException("Next handler error");
        
        await repository.ExecuteAsync(Arg.Do<CheckSettleStatusMatchCriteria>(c =>
        {
            c.ErrCode = 0;
            c.ErrMsg = "Success";
        }));

        nextHandler.HandleAsync(match).Returns(Task.FromException<Result>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(match));
        
        Assert.Equal("Next handler error", exception.Message);
    }

    [Theory]
    [InlineData(0, "Success")]
    [InlineData(1, "General error")]
    [InlineData(100, "Custom error")]
    [InlineData(-1, "Negative error code")]
    public async Task HandleAsync_WithDifferentErrorCodes_ShouldReturnCorrectResult(int errCode, string errMsg)
    {
        // Arrange
        var match = CreateSampleMatch();
        
        await repository.ExecuteAsync(Arg.Do<CheckSettleStatusMatchCriteria>(c =>
        {
            c.ErrCode = errCode;
            c.ErrMsg = errMsg;
        }));

        // Setup nextHandler for success case
        if (errCode == 0)
        {
            nextHandler.HandleAsync(match).Returns(new Result { ErrCode = 0, ErrMsg = "Next handler success" });
        }

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        if (errCode == 0)
        {
            // When errCode is 0, the handler calls next handler and returns its result
            Assert.Equal(0, result.ErrCode);
            Assert.Equal("Next handler success", result.ErrMsg);
        }
        else
        {
            // When errCode is not 0, the handler returns the repository result
            Assert.Equal(errCode, result.ErrCode);
            Assert.Equal(errMsg, result.ErrMsg);
        }
        
        if (errCode == 0)
        {
            await nextHandler.Received(1).HandleAsync(match);
        }
        else
        {
            await nextHandler.DidNotReceive().HandleAsync(Arg.Any<Match>());
        }
    }

    private static Match CreateSampleMatch()
    {
        return new Match
        {
            MatchId = 12345,
            LeagueId = 100,
            SportType = 1,
            ChildMatchType = 1,
            ResultDetails = new List<ResultDetail>
            {
                new() { ResourceId = "R1", HomeValue = "1.5", AwayValue = "2.5" },
                new() { ResourceId = "R2", HomeValue = "1.8", AwayValue = "2.0" }
            }
        };
    }
}
