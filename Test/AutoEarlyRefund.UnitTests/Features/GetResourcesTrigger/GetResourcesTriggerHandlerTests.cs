using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.GetResourcesTrigger;

public class GetResourcesTriggerHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly GetResourcesTriggerHandler handler;

    public GetResourcesTriggerHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        handler = new GetResourcesTriggerHandler(repository);
    }

    [Fact]
    public async Task GetTargetResourceTriggersAsync_WithCriteria_ShouldReturnResults()
    {
        // Arrange
        var criteria = new GetResourcesTriggerCriteria
        {
            Action = "Cal",
            SportType = 1,
            ChildMatchType = 2,
            ResourceIds = "R1,R2"
        };

        var expectedResults = new List<TargetResourceTrigger>
        {
            new() { TargetResource = "R1" },
            new() { TargetResource = "R2" }
        };

        repository.FetchAsync<TargetResourceTrigger>(criteria)
            .Returns(expectedResults);

        // Act
        var result = await handler.GetTargetResourceTriggersAsync(criteria);

        // Assert
        Assert.Equal(expectedResults, result);
        await repository.Received(1).FetchAsync<TargetResourceTrigger>(criteria);
    }

    [Fact]
    public async Task GetTargetResourceTriggersAsync_WithParameters_ShouldCreateCorrectCriteria()
    {
        // Arrange
        var action = "Cal";
        var shortType = (short)1;
        var childMatchType = (short)2;
        var resourceIds = "R1,R2";

        var expectedResults = new List<TargetResourceTrigger>
        {
            new() { TargetResource = "R1" },
            new() { TargetResource = "R2" }
        };

        repository.FetchAsync<TargetResourceTrigger>(Arg.Is<GetResourcesTriggerCriteria>(c =>
            c.Action == action &&
            c.SportType == shortType &&
            c.ChildMatchType == childMatchType &&
            c.ResourceIds == resourceIds))
            .Returns(expectedResults);

        // Act
        var result = await handler.GetTargetResourceTriggersAsync(action, shortType, childMatchType, resourceIds);

        // Assert
        Assert.Equal(expectedResults, result);
        await repository.Received(1).FetchAsync<TargetResourceTrigger>(
            Arg.Is<GetResourcesTriggerCriteria>(c =>
                c.Action == action &&
                c.SportType == shortType &&
                c.ChildMatchType == childMatchType &&
                c.ResourceIds == resourceIds));
    }

    [Fact]
    public async Task GetTargetResourceTriggersAsync_WhenNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var criteria = new GetResourcesTriggerCriteria
        {
            Action = "Cal",
            SportType = 1,
            ChildMatchType = 2,
            ResourceIds = "R1"
        };

        repository.FetchAsync<TargetResourceTrigger>(criteria)
            .Returns(new List<TargetResourceTrigger>());

        // Act
        var result = await handler.GetTargetResourceTriggersAsync(criteria);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTargetResourceTriggersAsync_WithParameters_WhenNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        repository.FetchAsync<TargetResourceTrigger>(Arg.Any<GetResourcesTriggerCriteria>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        var result = await handler.GetTargetResourceTriggersAsync("Cal", 1, 2, "R1");

        // Assert
        Assert.Empty(result);
    }
}