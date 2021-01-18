using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class MatchServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly IMatchService service;

        public MatchServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            service = new MatchServiceImp(repository);
        }

        [Fact]
        public void GetMatches_ExistedIncludeLeagueIdsAndExcludeLeagueIds_ReturnsRightMatches()
        {
            // Arrange
            IEnumerable<Match> matches = new List<Match>
            {
                new Match { MatchId = 1, LeagueId = 123 },
                new Match { MatchId = 2, LeagueId = 1111 }
            };
            repository.Fetch<Match>(Arg.Any<MatchListingCriteria>()).Returns(matches);

            // Act
            var list = service.GetMatches();

            // Assert
            Assert.Single(list);
        }

        [Fact]
        public void UpdateStatus_Aways_ExecuteCriteria()
        {
            // Arrange
            var match = new Match { MatchId = 1 };

            // Act
            service.UpdateStatus(match, 1);

            // Assert
            repository.Received().Execute(Arg.Any<UpdateStatusCriteria>());

        }

        [Fact]
        public void CheckMissedMatch_Aways_ExecuteCriteria()
        {
            service.CheckMissedMatch();

            // Assert
            repository.Received().Execute(Arg.Any<UpdateStatusCriteria>());
        }
    }
}
