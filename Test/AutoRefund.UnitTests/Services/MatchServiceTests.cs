using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class MatchServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly IMatchService service;

        private readonly ICache cache;
        public MatchServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            cache = new RedisCache(new RedisCacheOptions
            {
                ConnectionString = $"ha.nexdev.net:16379,password=foo1@bared,defaultDatabase=11",
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

        [Fact]
        public void GetMatches_ExistedIncludeLeagueIdAndExcludeLeagueId_ReturnsRightMatches()
        {
            // Arrange
            IEnumerable<Match> matches = new List<Match>
            {
                new Match { MatchId = 1, LeagueId = 123 },
                new Match { MatchId = 2, LeagueId = 1 }
            };
            repository.Fetch<Match>(Arg.Any<MatchListingCriteria>()).Returns(matches);
            const string KEY_INCLUDE_LEAGUEIDS = "Settlement-AutoPayout-AutoInstant-IncludeLeagueId";
            const int CACHE_EXPIRED_IN_DAYS = 1095;
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(CACHE_EXPIRED_IN_DAYS));
            cache.Set(KEY_INCLUDE_LEAGUEIDS, 58087, cachePolicy);

            // Act
            var list = service.GetMatches();

            // Assert
            Assert.Single(list);
        }

        [Fact]
        public void UpdateStatus_Always_ExecuteCriteria()
        {
            // Arrange
            var match = new Match { MatchId = 1 };

            // Act
            service.UpdateStatus(match, 1);

            // Assert
            repository.Received().Execute(Arg.Any<UpdateStatusCriteria>());

        }

        [Fact]
        public void CheckMissedMatch_Always_ExecuteCriteria()
        {
            service.CheckMissedMatch();

            // Assert
            repository.Received().Execute(Arg.Any<UpdateStatusCriteria>());
        }
    }
}
