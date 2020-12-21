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
            var matchOne = new Match { MatchId = 1, LeagueId = 123 };
            var matchTwo = new Match { MatchId = 2, LeagueId = 456 };
            var list = new List<Match>();
            list.Add(matchOne);
            list.Add(matchTwo);
            repository.Fetch<Match>(new MatchListingCriteria()).Returns(list);

            // Act
            var matches = service.GetMatches();

            // Assert
            Assert.Single(matches);
        }
    }
}
