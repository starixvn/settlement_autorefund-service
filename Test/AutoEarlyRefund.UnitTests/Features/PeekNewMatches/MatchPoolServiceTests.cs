using AutoEarlyRefund.Features.PeekNewMatches;
using AutoEarlyRefund.Models;
using NSubstitute;
using Xunit;

namespace AutoEarlyRefund.UnitTests.Features.PeekNewMatches;

public class MatchPoolServiceTests
{
    private readonly GetMatchesService matchesService;
    private readonly MatchPoolService service;

    public MatchPoolServiceTests()
    {
        matchesService = Substitute.For<GetMatchesService>();
        service = new MatchPoolService(matchesService);
    }

    [Fact]
    public async Task PeekNewMatch_WhenNoExistingMatches_ShouldReturnAllMatches()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1 },
            new() { MatchId = 2 }
        };
        matchesService.GetMatches().Returns(matches);

        // Act
        var result = await service.PeekNewMatch();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.MatchId == 1);
        Assert.Contains(result, m => m.MatchId == 2);
    }

    [Fact]
    public async Task PeekNewMatch_WhenCalledTwice_ShouldOnlyReturnNewMatches()
    {
        // Arrange
        var firstCall = new List<Match> { new() { MatchId = 1 } };
        var secondCall = new List<Match>
        {
            new() { MatchId = 1 },
            new() { MatchId = 2 }
        };

        matchesService.GetMatches()
            .Returns(firstCall, secondCall);

        // Act
        await service.PeekNewMatch();
        var result = await service.PeekNewMatch();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].MatchId);
    }

    [Fact]
    public async Task Remove_ShouldRemoveMatchFromPool()
    {
        // Arrange
        var matches = new List<Match> { new() { MatchId = 1 } };
        matchesService.GetMatches().Returns(matches);
        await service.PeekNewMatch();

        // Act
        service.Remove(new Match { MatchId = 1 });
        var result = await service.PeekNewMatch();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].MatchId);
    }

    [Fact]
    public async Task PeekNewMatch_WithConcurrentAccess_ShouldHandleRaceConditions()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1 },
            new() { MatchId = 2 }
        };
        matchesService.GetMatches().Returns(matches);

        // Act
        var tasks = new List<Task<List<Match>>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(service.PeekNewMatch());
        }
        await Task.WhenAll(tasks);

        // Assert
        // First call should get both matches, subsequent calls should get none
        Assert.Equal(2, tasks[0].Result.Count);
        for (int i = 1; i < tasks.Count; i++)
        {
            Assert.Empty(tasks[i].Result);
        }
    }

    [Fact]
    public async Task Remove_WithConcurrentAccess_ShouldHandleRaceConditions()
    {
        // Arrange
        var matches = new List<Match> { new() { MatchId = 1 } };
        matchesService.GetMatches().Returns(matches);
        await service.PeekNewMatch();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => service.Remove(new Match { MatchId = 1 })));
        }
        await Task.WhenAll(tasks);

        var result = await service.PeekNewMatch();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].MatchId);
    }

    [Fact]
    public void Remove_WhenMatchDoesNotExist_ShouldNotThrowException()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            service.Remove(new Match { MatchId = 999 }));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    public void Remove_WithNullMatch_ShouldThrowArgumentNullException(Match match)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.Remove(match));
    }
}