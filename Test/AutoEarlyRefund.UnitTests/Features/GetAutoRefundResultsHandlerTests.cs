using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features;

public class GetAutoRefundResultsHandlerTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly GetAutoRefundResultsHandler sut;

    public GetAutoRefundResultsHandlerTests()
    {
        sut = new GetAutoRefundResultsHandler(repository);
    }

    [Fact]
    public async Task Handle_NoResultsFromRepository_ReturnsAllSourceConditions()
    {
        // Arrange
        var matchIds = new List<int> { 1, 2 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new SourcesConditionsItem { ResourceId = "A" },
            new SourcesConditionsItem { ResourceId = "B" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>()).Returns(new List<ResultDetail>());

        // Act
        var result = await sut.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.ResourceId == "A");
        Assert.Contains(result, x => x.ResourceId == "B");
    }

    [Fact]
    public async Task Handle_RepositoryReturnsSomeResults_FiltersOutExistingResourceIds()
    {
        // Arrange
        var matchIds = new List<int> { 1 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new SourcesConditionsItem { ResourceId = "A" },
            new SourcesConditionsItem { ResourceId = "B" },
            new SourcesConditionsItem { ResourceId = "C" }
        };
        var repoResults = new List<ResultDetail>
        {
            new ResultDetail { ResourceId = "B" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>()).Returns(repoResults);

        // Act
        var result = await sut.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.ResourceId == "A");
        Assert.Contains(result, x => x.ResourceId == "C");
        Assert.DoesNotContain(result, x => x.ResourceId == "B");
    }

    [Fact]
    public async Task Handle_EmptyMatchIds_ReturnsAllSourceConditions()
    {
        // Arrange
        var matchIds = new List<int>();
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new SourcesConditionsItem { ResourceId = "A" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>()).Returns(new List<ResultDetail>());

        // Act
        var result = await sut.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Single(result);
        Assert.Equal("A", result[0].ResourceId);
    }

    [Fact]
    public async Task Handle_EmptySourcesConditions_ReturnsEmptyList()
    {
        // Arrange
        var matchIds = new List<int> { 1 };
        var sourcesConditions = new List<SourcesConditionsItem>();
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>()).Returns(new List<ResultDetail>());

        // Act
        var result = await sut.Handle(matchIds, sourcesConditions);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithSerializedCriteria()
    {
        // Arrange
        var matchIds = new List<int> { 1 };
        var sourcesConditions = new List<SourcesConditionsItem>
        {
            new SourcesConditionsItem { ResourceId = "A" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetAutoRefundResultsCriteria>()).Returns(new List<ResultDetail>());

        // Act
        await sut.Handle(matchIds, sourcesConditions);

        // Assert
        await repository.Received(1).FetchAsync<ResultDetail>(Arg.Is<GetAutoRefundResultsCriteria>(c =>
            !string.IsNullOrEmpty(c.ResourceInfo)));
    }
}
