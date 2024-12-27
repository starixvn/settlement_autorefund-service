using AutoRefund.Core;
using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class MatchServiceTests
{
    private readonly IDynamicRepository repository;

    private IMatchService service;

    private readonly ICache cache;

    public MatchServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        cache = new RedisCache(new RedisCacheOptions
        {
            ConnectionString = "10.18.200.131:16379,password=foo1@bared,defaultDatabase=11",
            ApplicationName = "Settlement-LOCAL"
        });

        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 15,
            EnvironmentName = "PRO"
        };

        var appSettingsOptions = Options.Create(settings);
        service = new MatchServiceImp(repository, cache, appSettingsOptions);
    }

    private async Task SetLeague(int leagueId)
    {
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1));
        await cache.SetAsync(CacheKey.LeagueId, leagueId, cachePolicy);
    }

    private static List<Match> MatchesTest =>
        new()
        {
            new Match { MatchId = 1, LeagueId = 123 },
            new Match { MatchId = 2, LeagueId = 1 },
            new Match { MatchId = 2, LeagueId = 2 }
        };

    [Fact]
    public async Task GetMatches_EnvironmentNameIsPROAndExistLeagueTest_ReturnsExcludeLeagueTest()
    {
        // Arrange
        await SetLeague(123);

        repository.FetchAsync<Match>(Arg.Any<MatchListingCriteria>()).Returns(MatchesTest);

        // Act
        var list = await service.GetMatches();

        // Assert
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetMatches_EnvironmentNameIsPROAndNotExistLeagueTest_ReturnAllLeagues()
    {
        // Arrange
        await SetLeague(0);
        repository.FetchAsync<Match>(Arg.Any<MatchListingCriteria>()).Returns(MatchesTest);

        // Act
        var list = await service.GetMatches();

        // Assert
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task GetMatches_EnvironmentNameIsUAT_ReturnsIncludeLeagueTest()
    {
        // Arrange

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "UAT"
        };

        var appSettingsOptions = Options.Create(settings);
        service = new MatchServiceImp(repository, cache, appSettingsOptions);

        await SetLeague(123);

        repository.FetchAsync<Match>(Arg.Any<MatchListingCriteria>()).Returns(MatchesTest);

        // Act
        var list = await service.GetMatches();

        // Assert
        Assert.Single(list);
    }

    [Fact]
    public async Task GetMatches_EnvironmentNameIsLOCAL_ReturnsAllLeagueTest()
    {
        // Arrange

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "LOCAL"
        };

        var appSettingsOptions = Options.Create(settings);
        service = new MatchServiceImp(repository, cache, appSettingsOptions);

        await SetLeague(123);

        repository.FetchAsync<Match>(Arg.Any<MatchListingCriteria>()).Returns(MatchesTest);

        // Act
        var list = await service.GetMatches();

        // Assert
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task UpdateStatus_Always_ExecuteCriteria()
    {
        // Arrange
        var match = new Match { MatchId = 1 };

        // Act
        await service.UpdateStatus(match, 1);

        // Assert
        await repository.Received().ExecuteAsync(Arg.Any<UpdateStatusCriteria>());
    }

    [Fact]
    public async Task UpdateRemark_Always_ExecuteCriteria()
    {
        // Arrange
        var match = new Match { MatchId = 1 };

        // Act
        await service.UpdateRemark(match, "", 0);

        // Assert
        await repository.Received().ExecuteAsync(Arg.Any<UpdateStatusCriteria>());
    }

    [Fact]
    public async Task GetMatchInfo_Always_InvokeGetAsyncMethod()
    {
        // Arrange
        var match = new Match { MatchId = 1 };

        // Act
        await service.GetMatchInfo(match);

        // Assert
        await repository.Received().GetAsync<MatchChecking>(Arg.Any<MatchCheckingCriteria>());
    }

    [Fact]
    public async Task CheckMissedMatch_Always_ExecuteCriteria()
    {
        await service.CheckMissedMatch();

        // Assert
        await repository.Received().ExecuteAsync(Arg.Any<UpdateStatusCriteria>());
    }
}