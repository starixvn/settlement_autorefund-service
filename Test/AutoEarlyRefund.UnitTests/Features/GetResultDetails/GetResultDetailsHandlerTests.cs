using AutoEarlyRefund.Features.GetRefundResults;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace AutoEarlyRefund.UnitTests.Features.GetResultDetails;

public class GetResultDetailsHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IGetResultDetailsHandler handler;

    public GetResultDetailsHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        handler = new GetResultDetailsHandler(repository);
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldReturnFilteredResults()
    {
        // Arrange
        var matchId = 1;
        var triggers = new List<TargetResourceTrigger>
        {
            new()
            {
                Id = 1,
                SportType = 1,
                MatchType = 1,
                TargetResourceChildMatchType = 2,
                TargetResource = "R1,R2"
            }
        };

        var results = new List<ResultDetail>
        {
            new() { ResourceId = "R1", ResultStatusId = (int)ResultStatus.ManualSettled },
            new() { ResourceId = "R3", ResultStatusId = (int)ResultStatus.AutoSettled }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetResultDetailsCriteria>())
            .Returns(results);

        // Act
        var result = await handler.Handle(matchId, triggers);

        // Assert
        Assert.Single(result);
        Assert.Equal("R2", result[0].ResourceId);
    }

    [Fact]
    public async Task Handle_WithSettledResults_ShouldFilterThemOut()
    {
        // Arrange
        var matchId = 1;
        var triggers = new List<TargetResourceTrigger>
        {
            new()
            {
                SportType = 1,
                MatchType = 1,
                TargetResourceChildMatchType = 2,
                TargetResource = "R1"
            }
        };

        var results = new List<ResultDetail>
        {
            new() { ResourceId = "R1", ResultStatusId = (int)ResultStatus.ManualSettled }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetResultDetailsCriteria>())
            .Returns(results);

        // Act
        var result = await handler.Handle(matchId, triggers);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WithInvalidMatchId_ShouldThrowArgumentException(int matchId)
    {
        // Arrange
        var triggers = new List<TargetResourceTrigger>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(matchId, triggers));
    }

    [Fact]
    public async Task Handle_WithNullTriggers_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.Handle(1, null!));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetResultDetailsHandler(null!));
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectCriteriaAndJsonSerialization()
    {
        // Arrange
        var matchId = 1;
        var triggers = new List<TargetResourceTrigger>
        {
            new()
            {
                SportType = 1,
                MatchType = 1,
                TargetResourceChildMatchType = 2,
                TargetResource = "R1"
            }
        };

        // Act
        await handler.Handle(matchId, triggers);

        // Assert
        await repository.Received(1).FetchAsync<ResultDetail>(
            Arg.Is<GetResultDetailsCriteria>(c =>
                c.ResourceInfo.Contains("\"matchId\":1") &&
                c.ResourceInfo.Contains("\"sportType\":1") &&
                c.ResourceInfo.Contains("\"resourceId\":\"R1\"")));
    }
}