using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class MatchPoolServiceTests
    {
        private readonly IMatchPoolService service;

        private readonly IMatchService matchService;

        public MatchPoolServiceTests()
        {
            matchService = Substitute.For<IMatchService>();
            service = new MatchPoolServiceImp(matchService);
        }

        private List<Match> CreateMatches(params int[] matchIds)
        {
            return matchIds.Select(i => new Match { MatchId = i}).ToList();
        }

        [Fact]
        public void FetchNewMatch_Twice_OnlyAddNewMatchToPool()
        {
            matchService.GetMatches().Returns(CreateMatches(1, 3));
            service.PeekNewMatch();

            matchService.GetMatches().Returns(CreateMatches(2, 3));
            var listMatch = service.PeekNewMatch();

            Assert.Equal(3, listMatch.Count);
        }

        [Fact]
        public void Remove_ExistedMatchId_ExistingMatchIsRemove()
        {
            // Arrange
            var matches = CreateMatches(1, 2);
            var serviceMock = new MatchPoolServiceImp(matchService);
            matchService.GetMatches().Returns(matches);

            // Act1
            serviceMock.PeekNewMatch();
            serviceMock.Remove(matches[0]);

            // Assert
            Assert.Single(serviceMock.PoolOfMatches);
        }

        [Fact]
        public void RemoveAndUpdateStatus_ExistedMatchId_ExistingMatchIsRemoveAndUpdateStatus()
        {
            // Arrange
            var matches = CreateMatches(1, 2);
            var serviceMock = new MatchPoolServiceImp(matchService);
            matchService.GetMatches().Returns(matches);

            // Act1
            serviceMock.PeekNewMatch();
            serviceMock.RemoveAndUpdateStatus(matches[0]);

            // Assert
            Assert.Single(serviceMock.PoolOfMatches);
            matchService.Received().UpdateStatus(matches[0]);
        }
    }
}
