using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.GetAutoRefundResults;

public class GetAutoRefundResultsHandlerTests : TestBase
{
    private readonly IDynamicRepository repository;
    private readonly GetAutoRefundResultsHandler handler;

    public GetAutoRefundResultsHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        handler = new GetAutoRefundResultsHandler(repository);
    }

    [Fact]
    public async Task Handle_WithValidInputs_ShouldReturnFilteredSourcesConditions()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R3", SportType = 1, ChildMatchType = 1 }
        };

        var existingResults = new List<ResultDetail>
        {
            new() { ResourceId = "R1", MatchId = 1 },
            new() { ResourceId = "R2", MatchId = 2 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(existingResults);

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Single(result);
        Assert.Equal("R3", result[0].ResourceId);
        Assert.Equal(1, result[0].SportType);
        Assert.Equal(1, result[0].ChildMatchType);
    }

    [Fact]
    public async Task Handle_WithNoExistingResults_ShouldReturnAllSourcesConditions()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>());

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("R1", result[0].ResourceId);
        Assert.Equal("R2", result[1].ResourceId);
    }

    [Fact]
    public async Task Handle_WithAllResultsExisting_ShouldReturnEmptyList()
    {
        // Arrange
        var matchIds = new List<int> { 1 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 }
        };

        var existingResults = new List<ResultDetail>
        {
            new() { ResourceId = "R1", MatchId = 1 },
            new() { ResourceId = "R2", MatchId = 1 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(existingResults);

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithEmptyMatchIds_ShouldReturnAllSourcesConditions()
    {
        // Arrange
        var matchIds = new List<int>();
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>());

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Single(result);
        Assert.Equal("R1", result[0].ResourceId);
    }

    [Fact]
    public async Task Handle_WithEmptySourcesConditions_ShouldReturnEmptyList()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2 };
        var sourcesConditions = new List<SourcesConditionsItem>();

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>());

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldCreateCorrectCriteria()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 }
        };

        GetAutoRefundResultsCriteria? capturedCriteria = null;
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>())
            .AndDoes(call => capturedCriteria = call.Arg<GetAutoRefundResultsCriteria>());

        // Act
        await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.NotNull(capturedCriteria);
        Assert.False(string.IsNullOrWhiteSpace(capturedCriteria.ResourceInfo));
        
        // Verify the JSON contains the expected structure
        Assert.Contains("matchId", capturedCriteria.ResourceInfo);
        Assert.Contains("resourceId", capturedCriteria.ResourceInfo);
        Assert.Contains("1", capturedCriteria.ResourceInfo);
        Assert.Contains("2", capturedCriteria.ResourceInfo);
        Assert.Contains("R1", capturedCriteria.ResourceInfo);
        Assert.Contains("R2", capturedCriteria.ResourceInfo);
    }

    [Fact]
    public async Task Handle_ShouldCreateCorrectNumberOfRequestItems()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2, 3 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 }
        };

        GetAutoRefundResultsCriteria? capturedCriteria = null;
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>())
            .AndDoes(call => capturedCriteria = call.Arg<GetAutoRefundResultsCriteria>());

        // Act
        await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.NotNull(capturedCriteria);
        
        // Should create 3 matchIds * 2 sourcesConditions = 6 request items
        var requestItems = System.Text.Json.JsonSerializer.Deserialize<List<GetAutoRefundResultsItem>>(
            capturedCriteria.ResourceInfo, 
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        
        Assert.NotNull(requestItems);
        Assert.Equal(6, requestItems.Count);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var matchIds = new List<int> { 1 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 }
        };

        var expectedException = new InvalidOperationException("Database error");
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(Task.FromException<IEnumerable<ResultDetail>>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(matchIds, sourcesConditions));
        
        Assert.Equal("Database error", exception.Message);
    }

    [Theory]
    [InlineData(1, 1, 1)] // 1 matchId, 1 sourceCondition = 1 request item
    [InlineData(2, 3, 6)] // 2 matchIds, 3 sourceConditions = 6 request items
    [InlineData(5, 2, 10)] // 5 matchIds, 2 sourceConditions = 10 request items
    public async Task Handle_ShouldCreateCorrectRequestItemsCount(int matchIdCount, int sourceConditionCount, int expectedRequestItemCount)
    {
        // Arrange
        var matchIds = Enumerable.Range(1, matchIdCount).ToList();
        var sourcesConditions = Enumerable.Range(1, sourceConditionCount)
            .Select(i => new SourcesConditionsItem { ResourceId = $"R{i}", SportType = 1, ChildMatchType = 1 })
            .ToList();

        GetAutoRefundResultsCriteria? capturedCriteria = null;
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>())
            .AndDoes(call => capturedCriteria = call.Arg<GetAutoRefundResultsCriteria>());

        // Act
        await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.NotNull(capturedCriteria);
        
        var requestItems = System.Text.Json.JsonSerializer.Deserialize<List<GetAutoRefundResultsItem>>(
            capturedCriteria.ResourceInfo, 
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        
        Assert.NotNull(requestItems);
        Assert.Equal(expectedRequestItemCount, requestItems.Count);
    }

    [Fact]
    public async Task Handle_WithDuplicateResourceIds_ShouldFilterCorrectly()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 },
            new() { ResourceId = "R1", SportType = 1, ChildMatchType = 1 }, // Duplicate
            new() { ResourceId = "R2", SportType = 1, ChildMatchType = 1 }
        };

        var existingResults = new List<ResultDetail>
        {
            new() { ResourceId = "R1", MatchId = 1 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(existingResults);

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Single(result);
        Assert.Equal("R2", result[0].ResourceId);
    }

    [Fact]
    public async Task Handle_ShouldPreserveSourcesConditionsProperties()
    {
        // Arrange
        var matchIds = new List<int> { 1 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new() 
            { 
                ResourceId = "R1", 
                SportType = 2, 
                ChildMatchType = 3, 
                MatchType = 4, 
                RuleId = 5, 
                IsRefundSdt = true 
            }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>())
            .Returns(new List<ResultDetail>());

        // Act
        var result = await handler.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Single(result);
        var returnedItem = result[0];
        Assert.Equal("R1", returnedItem.ResourceId);
        Assert.Equal(2, returnedItem.SportType);
        Assert.Equal(3, returnedItem.ChildMatchType);
        Assert.Equal(4, returnedItem.MatchType);
        Assert.Equal(5, returnedItem.RuleId);
        Assert.True(returnedItem.IsRefundSdt);
    }
}
