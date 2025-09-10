using AutoRefund.Core.Models;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class GetFastMarketResultsHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> options;
    private readonly IFilterWaterBreakService filterWaterBreakService;
    private readonly IEventCounterService eventCounterService;
    private readonly GetFastMarketResultsHandler sut;

    public GetFastMarketResultsHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        options = Substitute.For<IOptions<AppSettingsKey>>();
        filterWaterBreakService = Substitute.For<IFilterWaterBreakService>();
        eventCounterService = Substitute.For<IEventCounterService>();

        var appSettingsKey = new AppSettingsKey
        {
            SiteDbServer = new List<string> { "server1", "server2" }
        };
        options.Value.Returns(appSettingsKey);

        sut = new GetFastMarketResultsHandler(repository, options, filterWaterBreakService, eventCounterService);
    }

    #region Handle Method Tests

    [Fact]
    public async Task Handle_WithEmptyFastMarketResults_ReturnsOnlyTicketResults()
    {
        // Arrange
        var session = new SpecialSession
        {
            MatchId = 123,
            HomeValue = "10",
            AwayValue = "15"
        };

        var eventCounters = new List<EventCounter>();
        var fastMarketResults = new List<FastMarketResultDetail>();
        var ticketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, Result = 1 }
        };

        var filteredResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, Result = 1 }
        };

        var filteredEventCounters = new List<EventCounter>();

        SetupRepositoryMocks(session.MatchId, eventCounters, "handicap", fastMarketResults);
        SetupTicketDetailMocks(session.MatchId, ticketResults);
        SetupFilterMocks(filteredResults, filteredEventCounters, session);

        // Act
        var (results, eventCountersResult) = await sut.Handle(session);

        // Assert
        Assert.Single(results);
        Assert.Empty(eventCountersResult);
    }

    #endregion

    #region Private Method Tests

    [Fact]
    public void FilterResultsByWaterBreakAndStatus_WithValidResults_ReturnsFilteredResults()
    {
        // Arrange
        var session = new SpecialSession
        {
            HomeValue = "10",
            AwayValue = "15"
        };

        var combinedResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, ResultStatus = 0 },
            new FastMarketResultDetail { BetType = 222, Position = 11, ResultStatus = 4 },
            new FastMarketResultDetail { BetType = 223, Position = 12, ResultStatus = 6 }
        };

        var filteredResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, ResultStatus = 0 },
            new FastMarketResultDetail { BetType = 222, Position = 11, ResultStatus = 4 }
        };

        filterWaterBreakService.ApplyFilterByWaterBreak(Arg.Any<IEnumerable<FastMarketResultDetail>>(), session)
            .Returns(filteredResults);

        // Act
        var result = typeof(GetFastMarketResultsHandler)
            .GetMethod("FilterResultsByWaterBreakAndStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(sut, new object[] { combinedResults, session }) as List<FastMarketResultDetail>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.ResultStatus == 0);
        Assert.Contains(result, r => r.ResultStatus == 4);
        Assert.DoesNotContain(result, r => r.ResultStatus == 6);
    }

    [Fact]
    public void SetResultWithLabelsAndCounters_WithValidResults_SetsLabelsAndCounters()
    {
        // Arrange
        var results = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, Result = 1 }
        };

        var eventCounters = new List<EventCounter>
        {
            new EventCounter { Position = 10, Result = "1000000" }
        };

        var expectedEventCounters = new List<EventCounter>
        {
            new EventCounter { Position = 10, Result = "1000000" }
        };

        eventCounterService.GetEventCountersForBetType(Arg.Any<FastMarketResultDetail>(), eventCounters)
            .Returns(expectedEventCounters);

        // Act
        typeof(GetFastMarketResultsHandler)
            .GetMethod("SetResultWithLabelsAndCounters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(sut, new object[] { results, "handicap", eventCounters });

        // Assert
        Assert.NotNull(results[0].ResultItemLabel);
        Assert.Single(results[0].EventCounters);
        eventCounterService.Received(1).GetEventCountersForBetType(results[0], eventCounters);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task Handle_WithZeroMatchId_HandlesGracefully()
    {
        // Arrange
        var session = new SpecialSession
        {
            MatchId = 0,
            HomeValue = "10",
            AwayValue = "15"
        };

        var eventCounters = new List<EventCounter>();
        var fastMarketResults = new List<FastMarketResultDetail>();
        var ticketResults = new List<FastMarketResultDetail>();

        SetupRepositoryMocks(session.MatchId, eventCounters, "handicap", fastMarketResults);
        SetupTicketDetailMocks(session.MatchId, ticketResults);
        SetupFilterMocks(new List<FastMarketResultDetail>(), new List<EventCounter>(), session);

        // Act
        var (results, eventCountersResult) = await sut.Handle(session);

        // Assert
        Assert.Empty(results);
        Assert.Empty(eventCountersResult);
    }



    #endregion

    #region Helper Methods

    private void SetupRepositoryMocks(int matchId, List<EventCounter> eventCounters, string handicapValue, List<FastMarketResultDetail> fastMarketResults)
    {
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Is<GetFastMarketResultsCriteria>(c => c.MatchId == matchId))
            .Returns((eventCounters, new[] { handicapValue }, fastMarketResults, new List<ResultDetail>()));
    }

    private void SetupTicketDetailMocks(int matchId, List<FastMarketResultDetail> ticketResults)
    {
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Is<GetFastMarketTicketDetailCriteria>(c => c.MatchId == matchId))
            .Returns((new List<TicketCounts>(), ticketResults));
    }

    private void SetupFilterMocks(List<FastMarketResultDetail> filteredResults, List<EventCounter> filteredEventCounters, SpecialSession session)
    {
        filterWaterBreakService.ApplyFilterByWaterBreak(Arg.Any<IEnumerable<FastMarketResultDetail>>(), session)
            .Returns(filteredResults);

        filterWaterBreakService.ApplyFilterEventCountersByWaterBreak(Arg.Any<List<EventCounter>>(), session)
            .Returns(filteredEventCounters);
    }

    #endregion
}
