using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.GetResourcesConditions;

public class GetResourcesConditionsHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly GetResourcesConditionsHandler handler;

    public GetResourcesConditionsHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        handler = new GetResourcesConditionsHandler(repository);
    }

    [Fact]
    public async Task GetConditionsAsync_WithMatchType_ShouldSetMatchTypeAndReturnConditions()
    {
        // Arrange
        var matchType = (short)1;
        var items = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 }
        };

        var expectedConditions = new List<SourcesConditions>
        {
            new() { ResourceId = "R1" },
            new() { ResourceId = "R2" }
        };

        repository.FetchAsync<SourcesConditions>(Arg.Is<GetResourcesConditionsCriteria>(c =>
            c.ResourceInfo.Contains("matchType") &&
            c.ResourceInfo.Contains("\"matchType\":1")))
            .Returns(expectedConditions);

        // Act
        var result = await handler.GetConditionsAsync([matchType], items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

    }

    [Fact]
    public async Task GetConditionsAsync_WithItems_ShouldSerializeAndMapResults()
    {
        // Arrange
        var items = new List<SourcesConditionsItem>
        {
            new()
            {
                ResourceId = "R1",
                SportType = 1,
                ChildMatchType = 1,
                RuleId = 100
            }
        };

        var repoResults = new List<SourcesConditions>
        {
            new()
            {
                Id = 1,
                SportType = 1,
                ResourceId = "R1",
                RuleId = 100,
                RuleName = "Rule1",
                SourceAction = "Action1",
                MatchType = 1,
                ChildMatchType = 1,
                SourceMatchType = 2,
                SourceChildMatchType = 2,
                SourceResource = "Source1,Source2",
                Conditions = "Condition1"
            }
        };

        repository.FetchAsync<SourcesConditions>(Arg.Any<GetResourcesConditionsCriteria>())
            .Returns(repoResults);

        // Act
        var result = await handler.GetConditionsAsync(items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Two results because one source has two SourceResourceIds

        var firstResult = result.First();
        Assert.Equal(1, firstResult.Id);
        Assert.Equal(1, firstResult.SportType);
        Assert.Equal("R1", firstResult.ResourceId);
        Assert.Equal(100, firstResult.RuleId);
        Assert.Equal("Rule1", firstResult.RuleName);
        Assert.Equal("Action1", firstResult.SourceAction);
        Assert.Equal(1, firstResult.MatchType);
        Assert.Equal(1, firstResult.ChildMatchType);
        Assert.Equal(2, firstResult.SourceMatchType);
        Assert.Equal(2, firstResult.SourceChildMatchType);
        Assert.Equal("Source1,Source2", firstResult.SourceResource);
        Assert.Equal("Source1", firstResult.SourceResourceId);
        Assert.Equal("Condition1", firstResult.Conditions);
    }

    [Fact]
    public async Task GetConditionsAsync_WithEmptySourceResourceIds_ShouldReturnEmptyList()
    {
        // Arrange
        var items = new List<SourcesConditionsItem>
        {

        };

        var repoResults = new List<SourcesConditions>
        {

        };

        repository.FetchAsync<SourcesConditions>(Arg.Any<GetResourcesConditionsCriteria>())
            .Returns(repoResults);

        // Act
        var result = await handler.GetConditionsAsync(items);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetConditionsAsync_ShouldUseCorrectJsonSerialization()
    {
        // Arrange
        var items = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, RuleId = 100 }
        };

        // Act
        await handler.GetConditionsAsync(items);

        // Assert
        await repository.Received(1).FetchAsync<SourcesConditions>(
            Arg.Is<GetResourcesConditionsCriteria>(c =>
                c.ResourceInfo.Contains("resourceId") &&
                c.ResourceInfo.Contains("sportType") &&
                c.ResourceInfo.Contains("ruleId") &&
                !c.ResourceInfo.Contains("ResourceId") && // Verify camelCase
                !c.ResourceInfo.Contains("SportType") &&
                !c.ResourceInfo.Contains("RuleId")));
    }
}