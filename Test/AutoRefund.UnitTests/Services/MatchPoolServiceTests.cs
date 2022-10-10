using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class MatchPoolServiceTests
{
    private readonly IMatchPoolService service;

    private readonly IMatchService matchService;

    public MatchPoolServiceTests()
    {
        matchService = Substitute.For<IMatchService>();
        service = new MatchPoolServiceImp(matchService);
    }

    private static List<Match> CreateMatches(params int[] matchIds)
    {
        return matchIds.Select(i => new Match { MatchId = i }).ToList();
    }

    [Fact]
    public async Task FetchNewMatch_Twice_OnlyAddNewMatchToPool()
    {
        matchService.GetMatches().Returns(CreateMatches(1, 3));
        await service.PeekNewMatch();

        matchService.GetMatches().Returns(CreateMatches(2, 3));
        var listMatch = await service.PeekNewMatch();

        Assert.Single(listMatch);
    }

    [Fact]
    public async Task RemoveAndUpdateStatus_ExistedMatchId_ExistingMatchIsRemoveAndUpdateStatus()
    {
        // Arrange
        var matches = CreateMatches(1, 2);
        var serviceMock = new MatchPoolServiceImp(matchService);
        matchService.GetMatches().Returns(matches);

        // Act1
        await serviceMock.PeekNewMatch();
        serviceMock.RemoveAndUpdateStatus(matches[0], 1);

        // Assert
        Assert.Single(serviceMock.PoolOfMatches);
        await matchService.Received().UpdateStatus(matches[0], 1);
    }
}
