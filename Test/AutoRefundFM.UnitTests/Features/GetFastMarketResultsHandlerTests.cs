using AutoRefund.Core.Models;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class GetFastMarketResultsHandlerTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly IOptions<AppSettingsKey> options = Substitute.For<IOptions<AppSettingsKey>>();
    private readonly AppSettingsKey appSettingsKey = new() { SiteDbServer = new List<string> { "server1" } };
    private readonly GetFastMarketResultsHandler sut;

    public GetFastMarketResultsHandlerTests()
    {
        options.Value.Returns(appSettingsKey);
        sut = new GetFastMarketResultsHandler(repository, options);
    }

    [Fact]
    public async Task GetSpecialSessions_EmptySessions_ReturnsEmptyList()
    {
        // Arrange
        var match = new Match { MatchId = 1, SpecialSessions = new List<SpecialSession>() };
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, new List<FastMarketResultDetail>(), null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, new List<FastMarketResultDetail>()));

        // Act
        var result = await sut.GetSpecialSessions(match);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSpecialSessions_FiltersOutTimeMachineBetTypes()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 2,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession
                {
                    ResourceId = "R1",
                    StartMinute = 10,
                    EndMinute = 15,
                    FastMarketResultDetails = new List<FastMarketResultDetail>()
                }
            }
        };
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = (int)FastMarketBetType.TimeMachineHdp, Position = 12, ResultStatus = 1 },
            new FastMarketResultDetail { BetType = (int)FastMarketBetType.TimeMachineMinutes, Position = 13, ResultStatus = 1 },
            new FastMarketResultDetail { BetType = (int)FastMarketBetType.NextOneMinute, Position = 10, ResultStatus = 1 }
        };
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, new List<FastMarketResultDetail>()));

        // Act
        var result = await sut.GetSpecialSessions(match);

        // Assert
        Assert.Single(result);
        var session = result[0];
        Assert.Single(session.FastMarketResultDetails);
        Assert.Equal((int)FastMarketBetType.NextOneMinute, session.FastMarketResultDetails[0].BetType);
    }

    [Fact]
    public async Task GetSpecialSessions_ApplyFilterOneMinuteWaterBreak_MultiMinuteSession()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 3,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession
                {
                    ResourceId = "R2",
                    StartMinute = 5,
                    EndMinute = 7,
                    FastMarketResultDetails = new List<FastMarketResultDetail>()
                }
            }
        };
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = (int)FastMarketBetType.NextOneMinute, Position = 5, ResultStatus = 1 },
            new FastMarketResultDetail { BetType = (int)FastMarketBetType.NextOneMinute, Position = 6, ResultStatus = 1 },
            new FastMarketResultDetail { BetType = (int)FastMarketBetType.NextOneMinute, Position = 8, ResultStatus = 1 }
        };
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, new List<FastMarketResultDetail>()));

        // Act
        var result = await sut.GetSpecialSessions(match);

        // Assert
        Assert.Single(result);
        var session = result[0];
        Assert.Equal(2, session.FastMarketResultDetails.Count);
        Assert.All(session.FastMarketResultDetails, d => Assert.InRange(d.Position, 5, 7));
    }

    [Fact]
    public async Task GetSpecialSessions_ApplyFilterFiveMinuteWaterBreak()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 4,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession
                {
                    ResourceId = "R3",
                    StartMinute = 10,
                    EndMinute = 15,
                    FastMarketResultDetails = new List<FastMarketResultDetail>()
                }
            }
        };
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 99, Position = 1, ResultStatus = 1 },  // outside
            new FastMarketResultDetail { BetType = 99, Position = 11, ResultStatus = 1 },  // inside
            new FastMarketResultDetail { BetType = 99, Position = 15, ResultStatus = 1 }  // inside
        };
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, new List<FastMarketResultDetail>()));

        // Act
        var result = await sut.GetSpecialSessions(match);

        // Assert
        Assert.Single(result);
        var session = result[0];
        Assert.Equal(2, session.FastMarketResultDetails.Count);
        Assert.Contains(session.FastMarketResultDetails, d => d.Position == 11);
        Assert.Contains(session.FastMarketResultDetails, d => d.Position == 15);
    }

    [Fact]
    public async Task GetSpecialSessions_TicketResultsAreGroupedAndSummed()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 5,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession
                {
                    ResourceId = "R4",
                    StartMinute = 10,
                    EndMinute = 10,
                    FastMarketResultDetails = new List<FastMarketResultDetail>()
                }
            }
        };
        var fastMarketResults = new List<FastMarketResultDetail>();
        var ticketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 10, Result = 2, ResultStatus = 1, TicketCount = 1, TicketRunning = 1, TicketMarked = 0 },
            new FastMarketResultDetail { BetType = 1, Position = 10, Result = 3, ResultStatus = 1, TicketCount = 2, TicketRunning = 2, TicketMarked = 1 }
        };
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, ticketResults));

        // Act
        var result = await sut.GetSpecialSessions(match);

        // Assert
        Assert.Single(result);
        var session = result[0];
        Assert.Single(session.FastMarketResultDetails);
        var detail = session.FastMarketResultDetails[0];
        Assert.Equal(5, detail.Result); // 2 + 3
        Assert.Equal(3, detail.TicketCount); // 1 + 2
        Assert.Equal(3, detail.TicketRunning); // 1 + 2
        Assert.Equal(1, detail.TicketMarked); // 0 + 1
    }
}
