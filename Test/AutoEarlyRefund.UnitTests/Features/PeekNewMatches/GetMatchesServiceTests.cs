using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Models;
using AutoRefund.Core;
using AutoRefund.Core.Models;
using Fanex.Caching;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.PeekNewMatches;

public class GetMatchesServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly ICache cache;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly XmlConvertService xmlConvertService;
    private readonly GetMatchesServiceImp service;

    public GetMatchesServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        cache = Substitute.For<ICache>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        xmlConvertService = Substitute.For<XmlConvertService>();
        service = new GetMatchesServiceImp(repository, cache, appSettingsKey, xmlConvertService);
    }

    [Fact]
    public async Task GetMatches_ShouldGroupResultDetailsByMatchProperties()
    {
        // Arrange
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 100, SportType = 1, ChildMatchType = 1 },
            new() { MatchId = 1, LeagueId = 100, SportType = 1, ChildMatchType = 1 },
            new() { MatchId = 2, LeagueId = 200, SportType = 2, ChildMatchType = 2 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetPendingRefundResultsCriteria>())
            .Returns(resultDetails);
        cache.TryGetAsync<int>(CacheKey.LeagueId)
            .Returns((true, 0));
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "DEV" });

        // Act
        var result = await service.GetMatches();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.MatchId == 1 && m.LeagueId == 100);
        Assert.Contains(result, m => m.MatchId == 2 && m.LeagueId == 200);
    }

    [Theory]
    [InlineData("PRO", 100, true)]  // Should exclude matches with league ID 100
    [InlineData("PRO", 0, false)]   // Should include all matches when league ID is 0
    [InlineData("UAT", 100, false)] // Should include only matches with league ID 100
    [InlineData("DEV", 100, false)] // Should include all matches in DEV
    public async Task GetMatches_ShouldFilterBasedOnEnvironmentAndLeagueId(string env, int leagueId, bool shouldExclude)
    {
        // Arrange
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 100 },
            new() { MatchId = 2, LeagueId = 200 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetPendingRefundResultsCriteria>())
            .Returns(resultDetails);
        cache.TryGetAsync<int>(CacheKey.LeagueId)
            .Returns((true, leagueId));
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = env });

        // Act
        var result = await service.GetMatches();

        // Assert
        if (env == "UAT")
        {
            Assert.Single(result);
            Assert.Equal(100, result[0].LeagueId);
        }
        else if (env == "PRO" && leagueId > 0)
        {
            Assert.Single(result);
            Assert.Equal(200, result[0].LeagueId);
        }
        else
        {
            Assert.Equal(2, result.Count);
        }
    }

    [Fact]
    public async Task GetMatches_ShouldConvertResourcesToXml()
    {
        // Arrange
        var resultDetails = new List<ResultDetail>
        {
            new()
            {
                MatchId = 1,
                SourceId = "01",
                LeagueId = 100,
                ChildMatchType = 1,
                SportType = 1,
                HomeValue = "1" ,
                AwayValue = "0"
            }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetPendingRefundResultsCriteria>())
            .Returns(resultDetails);
        cache.TryGetAsync<int>(CacheKey.LeagueId)
            .Returns((true, 0));
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "DEV" });
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>())
            .Returns("<resources>test</resources>");

        // Act
        var matches = await service.GetMatches();

        // Assert
        Assert.Single(matches);
        Assert.Equal("<resources>test</resources>", matches[0].ResourcesXml);
    }

    [Fact]
    public async Task GetMatches_WhenCacheReturnsNoLeagueId_ShouldUseDefaultValue()
    {
        // Arrange
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 100 }
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetPendingRefundResultsCriteria>())
            .Returns(resultDetails);
        cache.TryGetAsync<int>(CacheKey.LeagueId)
            .Returns((false, 0));
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });

        // Act
        var result = await service.GetMatches();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetMatches_WhenRepositoryReturnsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        repository.FetchAsync<ResultDetail>(Arg.Any<GetPendingRefundResultsCriteria>())
            .Returns(new List<ResultDetail>());
        cache.TryGetAsync<int>(CacheKey.LeagueId)
            .Returns((true, 0));
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "DEV" });

        // Act
        var result = await service.GetMatches();

        // Assert
        Assert.Empty(result);
    }
}