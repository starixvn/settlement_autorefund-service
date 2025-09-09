using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Models;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class SessionPoolServiceTests
{
    private readonly IGetSpecialSessionHandler getSpecialSessionHandler = Substitute.For<IGetSpecialSessionHandler>();

    [Fact]
    public async Task PeekSessions_AddsNewSessionsAndReturnsOnlyNewOnes()
    {
        // Arrange
        var session1 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var session2 = new SpecialSession { MatchId = 2, ResourceId = "B" };
        getSpecialSessionHandler.Handle().Returns(new List<SpecialSession> { session1, session2 });
        var sut = new SessionPoolService(getSpecialSessionHandler);

        // Act
        var newSessions = await sut.PeekSessions();
        var pool = sut.PoolOfSessions;

        // Assert
        Assert.Equal(2, newSessions.Count);
        Assert.Contains(session1, newSessions);
        Assert.Contains(session2, newSessions);
        Assert.Equal(2, pool.Count);
    }

    [Fact]
    public async Task PeekSessions_DoesNotReturnAlreadyExistingSessions()
    {
        // Arrange
        var session1 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var session2 = new SpecialSession { MatchId = 2, ResourceId = "B" };
        getSpecialSessionHandler.Handle().Returns(new List<SpecialSession> { session1, session2 });
        var sut = new SessionPoolService(getSpecialSessionHandler);
        await sut.PeekSessions(); // Add both

        // Act
        getSpecialSessionHandler.Handle().Returns(new List<SpecialSession> { session1, session2 });
        var newSessions = await sut.PeekSessions();

        // Assert
        Assert.Empty(newSessions);
        Assert.Equal(2, sut.PoolOfSessions.Count);
    }

    [Fact]
    public async Task RemoveSession_RemovesSessionFromPool()
    {
        // Arrange
        var session1 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var session2 = new SpecialSession { MatchId = 2, ResourceId = "B" };
        getSpecialSessionHandler.Handle().Returns(new List<SpecialSession> { session1, session2 });
        var sut = new SessionPoolService(getSpecialSessionHandler);
        await sut.PeekSessions();

        // Act
        sut.RemoveSession(session1);
        var pool = sut.PoolOfSessions;

        // Assert
        Assert.Single(pool);
        Assert.DoesNotContain(session1, pool);
        Assert.Contains(session2, pool);
    }
}
