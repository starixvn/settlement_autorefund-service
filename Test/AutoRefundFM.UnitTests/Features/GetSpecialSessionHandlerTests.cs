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
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly ICache cache;

    public GetSpecialSessionHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        cache = Substitute.For<ICache>();
    }

    [Fact]
    public async Task Handle_ReturnsMatchesWithSpecialSessions()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        var resultDetails = new List<ResultDetail>
        {
            new() {
                MatchId = 1,
                ResourceId = "10",
                HomeValue = "12",
                AwayValue = "15",
                OtherInfo = "12:30,15:45"
            },
            new() {
                MatchId = 1,
                ResourceId = "11",
                HomeValue = "16",
                AwayValue = "20",
                OtherInfo = "16:10,20:20"
            },
            new() {
                MatchId = 2,
                ResourceId = "12",
                HomeValue = "5",
                AwayValue = "10",
                OtherInfo = "5:5,10:10"
            }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);

        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);

        // Act
        var matches = await handler.Handle();

        // Assert
        Assert.Equal(2, matches.Count);
        var match1 = matches.FirstOrDefault(m => m.MatchId == 1);
        Assert.NotNull(match1);
        Assert.Equal(2, match1.SpecialSessions.Count);
        Assert.Equal("10", match1.SpecialSessions[0].ResourceId);
        Assert.Equal(12, match1.SpecialSessions[0].StartMinute);
        Assert.Equal(30, match1.SpecialSessions[0].StartSecond);
        Assert.Equal(15, match1.SpecialSessions[0].EndMinute);
        Assert.Equal(45, match1.SpecialSessions[0].EndSecond);

        var match2 = matches.FirstOrDefault(m => m.MatchId == 2);
        Assert.NotNull(match2);
        Assert.Single(match2.SpecialSessions);
        Assert.Equal("12", match2.SpecialSessions[0].ResourceId);
    }

    [Fact]
    public async Task Handle_FiltersMatchesBasedOnEnv_PRO_WithLeagueId()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((true, 99));
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 99, ResourceId = "10", HomeValue = "12", AwayValue = "15", OtherInfo = "12:30,15:45" },
            new() { MatchId = 2, LeagueId = 100, ResourceId = "11", HomeValue = "16", AwayValue = "20", OtherInfo = "16:10,20:20" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act
        var matches = await handler.Handle();
        // Assert
        Assert.Single(matches);
        Assert.Equal(2, matches[0].MatchId);
    }

    [Fact]
    public async Task Handle_FiltersMatchesBasedOnEnv_PRO_WithoutLeagueId()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((false, 0));
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 99, ResourceId = "10", HomeValue = "12", AwayValue = "15", OtherInfo = "12:30,15:45" },
            new() { MatchId = 2, LeagueId = 100, ResourceId = "11", HomeValue = "16", AwayValue = "20", OtherInfo = "16:10,20:20" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act
        var matches = await handler.Handle();
        // Assert
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public async Task Handle_FiltersMatchesBasedOnEnv_UAT_WithLeagueId()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "UAT" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((true, 99));
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 99, ResourceId = "10", HomeValue = "12", AwayValue = "15", OtherInfo = "12:30,15:45" },
            new() { MatchId = 2, LeagueId = 100, ResourceId = "11", HomeValue = "16", AwayValue = "20", OtherInfo = "16:10,20:20" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act
        var matches = await handler.Handle();
        // Assert
        Assert.Single(matches);
        Assert.Equal(1, matches[0].MatchId);
    }

    [Fact]
    public async Task Handle_FiltersMatchesBasedOnEnv_DefaultEnv_ReturnsAll()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "DEV" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((true, 99));
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 99, ResourceId = "10", HomeValue = "12", AwayValue = "15", OtherInfo = "12:30,15:45" },
            new() { MatchId = 2, LeagueId = 100, ResourceId = "11", HomeValue = "16", AwayValue = "20", OtherInfo = "16:10,20:20" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act
        var matches = await handler.Handle();
        // Assert
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public async Task Handle_CacheMiss_ReturnsAllMatches()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((false, 0));
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 99, ResourceId = "10", HomeValue = "12", AwayValue = "15", OtherInfo = "12:30,15:45" },
            new() { MatchId = 2, LeagueId = 100, ResourceId = "11", HomeValue = "16", AwayValue = "20", OtherInfo = "16:10,20:20" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act
        var matches = await handler.Handle();
        // Assert
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyList()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((true, 99));
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(new List<ResultDetail>());
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act
        var matches = await handler.Handle();
        // Assert
        Assert.Empty(matches);
    }

    [Fact]
    public async Task Handle_MalformedOtherInfo_ThrowsFormatException()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        cache.TryGetAsync<int>(Arg.Any<string>()).Returns((true, 99));
        var resultDetails = new List<ResultDetail>
        {
            new() { MatchId = 1, LeagueId = 99, ResourceId = "10", HomeValue = "12", AwayValue = "15", OtherInfo = "InvalidFormat" }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);
        var handler = new GetSpecialSessionHandler(repository, appSettingsKey, cache);
        // Act & Assert
        await Assert.ThrowsAsync<IndexOutOfRangeException>(() => handler.Handle());
    }
}