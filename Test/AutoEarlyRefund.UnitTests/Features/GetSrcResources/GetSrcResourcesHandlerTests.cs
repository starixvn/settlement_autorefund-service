using AutoEarlyRefund.Features.GetSrcResources;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.GetSrcResources;

public class GetSrcResourcesHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IGetSrcResourcesHandler handler;

    public GetSrcResourcesHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        handler = new GetSrcResourcesHandler(repository);
    }

    [Fact]
    public async Task HandleAsync_WithValidInput_ShouldMapResourcesCorrectly()
    {
        // Arrange
        var matchId = 1;
        var sourcesConditions = new List<SourcesConditions>
        {
            new()
            {
                Id = 1,
                SportType = 1,
                ResourceId = "R1",
                SourceMatchType = 1,
                SourceChildMatchType = 2,
                SourceResource = "SR1",
                SourceResourceId = "SR1"
            }
        };

        var resources = new List<Resource>
        {
            new()
            {
                SourceResource = "SR1",
                SourceMatchType = 1,
                SourceChildMatchType = 2
            }
        };

        repository.FetchAsync<Resource>(Arg.Any<GetSrcResourcesCriteria>())
            .Returns(resources);

        // Act
        var result = await handler.HandleAsync([matchId], sourcesConditions);

        // Assert
        Assert.Single(result);
        var mappedCondition = result[0];
        Assert.Equal(1, mappedCondition.Id);
        Assert.Equal("R1", mappedCondition.ResourceId);
        Assert.NotNull(mappedCondition.Resource);
        Assert.Equal("SR1", mappedCondition.Resource.SourceResource);
    }

    [Fact]
    public async Task HandleAsync_WithNoMatchingResources_ShouldReturnConditionsWithNullResource()
    {
        // Arrange
        var matchId = 1;
        var sourcesConditions = new List<SourcesConditions>
        {
            new()
            {
                ResourceId = "R1",
                SourceResourceId = "SR1"
            }
        };

        repository.FetchAsync<Resource>(Arg.Any<GetSrcResourcesCriteria>())
            .Returns(new List<Resource>());

        // Act
        var result = await handler.HandleAsync([matchId], sourcesConditions);

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].Resource);
    }

    [Fact]
    public async Task HandleAsync_WithNullSourcesConditions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync([1], null!));
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectJsonSerialization()
    {
        // Arrange
        var matchId = 1;
        var sourcesConditions = new List<SourcesConditions>
        {
            new()
            {
                SportType = 1,
                SourceMatchType = 2,
                SourceChildMatchType = 3,
                SourceResource = "SR1"
            }
        };

        // Act
        await handler.HandleAsync([matchId], sourcesConditions);

        // Assert
        await repository.Received(1).FetchAsync<Resource>(
            Arg.Is<GetSrcResourcesCriteria>(c =>
                c.ResourceInfo.Contains("\"sportType\":1") &&
                c.ResourceInfo.Contains("\"sourceMatchType\":2") &&
                c.ResourceInfo.Contains("\"sourceChildMatchType\":3") &&
                c.ResourceInfo.Contains("\"sourceResource\":\"SR1\"") &&
                c.ResourceInfo.Contains("\"matchId\":1")));
    }
}