using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AutoEarlyRefund.UnitTests.Features.InsOrUpdAutoPayoutResult;

public class InsOrUpdAutoRefundResultHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IInsOrUpdAutoPayoutResultHandler handler;

    public InsOrUpdAutoRefundResultHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        handler = new InsOrUpdAutoRefundResultHandler(repository);
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldExecuteWithCorrectCriteria()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 123,
            ResourcesXml = "<resources>test</resources>"
        };
        var statusId = 1;

        // Act
        await handler.Handle(match, statusId);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<InsOrUpdAutoRefundResultCriteria>(c =>
            c.MatchId == match.MatchId &&
            c.StatusId == statusId &&
            c.SelectedResources == match.ResourcesXml));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var statusId = 1;
        var expectedException = new Exception("Repository error");

        repository.ExecuteAsync(Arg.Any<InsOrUpdAutoRefundResultCriteria>())
            .Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(match, statusId));

        Assert.Same(expectedException, exception);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new InsOrUpdAutoRefundResultHandler(null!));
    }

    [Theory]
    [InlineData(null, 1)]
    public async Task Handle_WithNullMatch_ShouldThrowArgumentNullException(Match match, int statusId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.Handle(match, statusId));
    }
}