using AutoRefund.Core;
using AutoRefund.Core.Models;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Models;
using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class GetSpecialSessionHandlerTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly IOptions<AppSettingsKey> options = Substitute.For<IOptions<AppSettingsKey>>();
    private readonly ICache cache = Substitute.For<ICache>();
    private readonly AppSettingsKey appSettingsKey = new() { EnvironmentName = "PRO" };

    private GetSpecialSessionHandler CreateSut() => new(repository, options, cache);

    [Fact]
    public async Task Handle_ReturnsFilteredAndDistinctSessions_PRO_LeagueId()
    {
        // Arrange
        options.Value.Returns(appSettingsKey);
        var repoResults = new List<ResultDetail>
        {
            new ResultDetail { MatchId = 1, LeagueId = 100, ResourceId = "A", HomeValue = "10", AwayValue = "20", OtherInfo = "S:1,E:2", ResultStatusId = 1 },
            new ResultDetail { MatchId = 2, LeagueId = 200, ResourceId = "B", HomeValue = "11", AwayValue = "21", OtherInfo = "S:3,E:4", ResultStatusId = 2 },
            new ResultDetail { MatchId = 1, LeagueId = 100, ResourceId = "A", HomeValue = "10", AwayValue = "20", OtherInfo = "S:1,E:2", ResultStatusId = 1 }, // duplicate
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(repoResults);
        cache.TryGetAsync<int>(CacheKey.LeagueId).Returns((true, 100));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle();

        // Assert
        Assert.Single(result); // Only LeagueId != 100 should remain
        Assert.Equal(2, result[0].MatchId);
        Assert.Equal("B", result[0].ResourceId);
    }

    [Fact]
    public async Task Handle_ReturnsAllSessions_WhenLeagueIdNotFound_PRO()
    {
        // Arrange
        options.Value.Returns(appSettingsKey);
        var repoResults = new List<ResultDetail>
        {
            new ResultDetail { MatchId = 1, LeagueId = 100, ResourceId = "A", HomeValue = "10", AwayValue = "20", OtherInfo = "S:1,E:2", ResultStatusId = 1 },
            new ResultDetail { MatchId = 2, LeagueId = 200, ResourceId = "B", HomeValue = "11", AwayValue = "21", OtherInfo = "S:3,E:4", ResultStatusId = 2 },
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(repoResults);
        cache.TryGetAsync<int>(CacheKey.LeagueId).Returns((false, 0));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.MatchId == 1 && x.ResourceId == "A");
        Assert.Contains(result, x => x.MatchId == 2 && x.ResourceId == "B");
    }

    [Fact]
    public async Task Handle_ReturnsFilteredSessions_UAT_OnlyMatchingLeagueId()
    {
        // Arrange
        appSettingsKey.EnvironmentName = "UAT";
        options.Value.Returns(appSettingsKey);
        var repoResults = new List<ResultDetail>
        {
            new ResultDetail { MatchId = 1, LeagueId = 100, ResourceId = "A", HomeValue = "10", AwayValue = "20", OtherInfo = "S:1,E:2", ResultStatusId = 1 },
            new ResultDetail { MatchId = 2, LeagueId = 200, ResourceId = "B", HomeValue = "11", AwayValue = "21", OtherInfo = "S:3,E:4", ResultStatusId = 2 },
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(repoResults);
        cache.TryGetAsync<int>(CacheKey.LeagueId).Returns((true, 200));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].MatchId);
        Assert.Equal("B", result[0].ResourceId);
    }

    [Fact]
    public async Task Handle_ReturnsAllSessions_WhenEnvironmentIsUnknown()
    {
        // Arrange
        appSettingsKey.EnvironmentName = "DEV";
        options.Value.Returns(appSettingsKey);
        var repoResults = new List<ResultDetail>
        {
            new ResultDetail { MatchId = 1, LeagueId = 100, ResourceId = "A", HomeValue = "10", AwayValue = "20", OtherInfo = "S:1,E:2", ResultStatusId = 1 },
            new ResultDetail { MatchId = 2, LeagueId = 200, ResourceId = "B", HomeValue = "11", AwayValue = "21", OtherInfo = "S:3,E:4", ResultStatusId = 2 },
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(repoResults);
        cache.TryGetAsync<int>(CacheKey.LeagueId).Returns((true, 100));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.MatchId == 1 && x.ResourceId == "A");
        Assert.Contains(result, x => x.MatchId == 2 && x.ResourceId == "B");
    }

    [Fact]
    public async Task Handle_ParsesStartAndEndSecondsCorrectly()
    {
        // Arrange
        options.Value.Returns(appSettingsKey);
        var repoResults = new List<ResultDetail>
        {
            new ResultDetail { MatchId = 1, LeagueId = 100, ResourceId = "A", HomeValue = "10", AwayValue = "20", OtherInfo = "S:7,E:8", ResultStatusId = 1 },
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(repoResults);
        cache.TryGetAsync<int>(CacheKey.LeagueId).Returns((false, 0));

        var sut = CreateSut();

        // Act
        var result = await sut.Handle();

        // Assert
        Assert.Single(result);
        Assert.Equal(7, result[0].StartSecond);
        Assert.Equal(8, result[0].EndSecond);
    }
}