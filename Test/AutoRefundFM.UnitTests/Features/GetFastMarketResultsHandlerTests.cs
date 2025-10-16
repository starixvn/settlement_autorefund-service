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


    [Fact]
    public async Task Handle_WithMultipleServers_CombinesTicketResults()
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

        // Mock multiple servers returning different ticket results
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((eventCounters, new[] { "handicap" }, fastMarketResults, new List<ResultDetail>()));

        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Is<GetFastMarketTicketDetailCriteria>(c => c.MatchId == 123 && c.DbServer == "server1"))
            .Returns((new List<TicketCounts>(), new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { BetType = 221, Position = 10, Result = 1, TicketCount = 5 }
            }));

        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Is<GetFastMarketTicketDetailCriteria>(c => c.MatchId == 123 && c.DbServer == "server2"))
            .Returns((new List<TicketCounts>(), new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { BetType = 221, Position = 10, Result = 2, TicketCount = 3 }
            }));

        var filteredResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, Result = 3, TicketCount = 8 }
        };

        var filteredEventCounters = new List<EventCounter>();

        SetupFilterMocks(filteredResults, filteredEventCounters, session);

        // Act
        var (results, eventCountersResult) = await sut.Handle(session);

        // Assert
        Assert.Single(results);
        Assert.Equal(3, results[0].Result); // Sum of results from both servers
        Assert.Equal(8, results[0].TicketCount); // Sum of ticket counts
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

    [Fact]
    public async Task Handle_AppliesEventCounterFiltering()
    {
        // Arrange
        var session = new SpecialSession { MatchId = 777, HomeValue = "10", AwayValue = "15" };

        var eventCounters = new List<EventCounter>
        {
            new() { Position = 1, Result = "1000000" },
            new() { Position = 2, Result = "2000000" }
        };
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new() { BetType = 221, Position = 1, ResultStatus = 0 }
        };
        var ticketResults = new List<FastMarketResultDetail>();

        var filteredResults = new List<FastMarketResultDetail>(fastMarketResults);
        var filteredEventCounters = new List<EventCounter> { eventCounters[0] }; // Only one survives filter

        SetupRepositoryMocks(session.MatchId, eventCounters, "handicap", fastMarketResults);
        SetupTicketDetailMocks(session.MatchId, ticketResults);
        SetupFilterMocks(filteredResults, filteredEventCounters, session);

        eventCounterService.GetEventCountersForBetType(Arg.Any<FastMarketResultDetail>(), filteredEventCounters)
            .Returns(ci => filteredEventCounters);

        // Act
        var (results, eventCountersResult) = await sut.Handle(session);

        // Assert
        Assert.Single(eventCountersResult);
        Assert.Single(results);
        Assert.Single(results[0].EventCounters);
    }

    [Fact]
    public void GetRadioResultFromResultItem_ReturnsExpectedMapping()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "GetRadioResultFromResultItem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var ri = new ResultItem { Corner = true };

        // Act
        var value = (int?)method?.Invoke(null, new object[] { ri });

        // Assert
        Assert.Equal((int)ResultType.Corners, value);
    }

    [Fact]
    public void CreateResultItemFromRadioResult_SetsCorrectFlag()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "CreateResultItemFromRadioResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var resultItem = method?.Invoke(null, new object[] { new FastMarketResultDetail { RadioResult = (int)ResultType.GoalKick } }) as ResultItem;

        // Assert
        Assert.NotNull(resultItem);
        Assert.True(resultItem!.GoalKick);
        Assert.False(resultItem.Goal);
        Assert.False(resultItem.Corner);
    }

    [Fact]
    public void CreateResultItemFromRadioResult_WithAllResultTypes_ReturnsCorrectFlags()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "CreateResultItemFromRadioResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Test all ResultType enum values
        var testCases = new[]
        {
            (ResultType.None, (Action<ResultItem>)(ri => Assert.True(ri.None))),
            (ResultType.Goals, (Action<ResultItem>)(ri => Assert.True(ri.Goal))),
            (ResultType.Corners, (Action<ResultItem>)(ri => Assert.True(ri.Corner))),
            (ResultType.FreeKick, (Action<ResultItem>)(ri => Assert.True(ri.FreeKick))),
            (ResultType.GoalKick, (Action<ResultItem>)(ri => Assert.True(ri.GoalKick))),
            (ResultType.ThrowIn, (Action<ResultItem>)(ri => Assert.True(ri.ThrowIn))),
            (ResultType.Bookings, (Action<ResultItem>)(ri => Assert.True(ri.Bookings))),
            (ResultType.Penalty, (Action<ResultItem>)(ri => Assert.True(ri.Penalty)))
        };

        foreach (var (resultType, assertion) in testCases)
        {
            // Act
            var resultItem = method?.Invoke(null, new object[] { new FastMarketResultDetail { RadioResult = (int)resultType } }) as ResultItem;

            // Assert
            Assert.NotNull(resultItem);
            assertion(resultItem!);
        }
    }

    [Fact]
    public void CreateResultItemFromRadioResult_WithInvalidResultType_ReturnsNoneFalse()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "CreateResultItemFromRadioResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var resultItem = method?.Invoke(null, new object[] { new FastMarketResultDetail { RadioResult = 999 } }) as ResultItem;

        // Assert
        Assert.NotNull(resultItem);
        Assert.False(resultItem!.None);
        Assert.False(resultItem.Goal);
        Assert.False(resultItem.Corner);
    }

    [Fact]
    public void GetRadioResultFromResultItem_WithAllResultItemTypes_ReturnsCorrectMapping()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "GetRadioResultFromResultItem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var testCases = new[]
        {
            (new ResultItem { None = true }, (int)ResultType.None),
            (new ResultItem { Goal = true }, (int)ResultType.Goals),
            (new ResultItem { Corner = true }, (int)ResultType.Corners),
            (new ResultItem { FreeKick = true }, (int)ResultType.FreeKick),
            (new ResultItem { GoalKick = true }, (int)ResultType.GoalKick),
            (new ResultItem { ThrowIn = true }, (int)ResultType.ThrowIn),
            (new ResultItem { Bookings = true }, (int)ResultType.Bookings),
            (new ResultItem { Penalty = true }, (int)ResultType.Penalty)
        };

        foreach (var (resultItem, expectedResult) in testCases)
        {
            // Act
            var result = (int?)method?.Invoke(null, new object[] { resultItem });

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }

    [Fact]
    public void GetRadioResultFromResultItem_WithAllFalse_ReturnsNone()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "GetRadioResultFromResultItem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var resultItem = new ResultItem(); // All false

        // Act
        var result = (int?)method?.Invoke(null, new object[] { resultItem });

        // Assert
        Assert.Equal((int)ResultType.None, result);
    }

    [Fact]
    public void SetResultItem_WithNonRadioBetType_SetsNewResultItem()
    {
        // Arrange
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "SetResultItem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var item = new FastMarketResultDetail
        {
            BetType = 221, // Non-radio bet type
            ResultItem = new ResultItem { Goal = true }
        };

        // Act
        method?.Invoke(null, [item]);

        // Assert
        Assert.NotNull(item.NewResultItem);
        Assert.False(item.NewResultItem.Goal);
    }

    [Fact]
    public async Task GetFastMarketTicketDetail_WithMultipleServers_CombinesResults()
    {
        // Arrange
        var session = new SpecialSession { MatchId = 123 };

        // Mock multiple servers with different results
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Is<GetFastMarketTicketDetailCriteria>(c => c.MatchId == 123 && c.DbServer == "server1"))
            .Returns((new List<TicketCounts>(), new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { BetType = 221, Position = 10, Result = 1, TicketCount = 5, TicketRunning = 2, TicketMarked = 1 }
            }));

        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Is<GetFastMarketTicketDetailCriteria>(c => c.MatchId == 123 && c.DbServer == "server2"))
            .Returns((new List<TicketCounts>(), new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { BetType = 221, Position = 10, Result = 2, TicketCount = 3, TicketRunning = 1, TicketMarked = 2 }
            }));

        // Act
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "GetFastMarketTicketDetail",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = await (Task<List<FastMarketResultDetail>>)method.Invoke(sut, [123]);

        // Assert
        Assert.Single(result);
        var combinedResult = result[0];
        Assert.Equal(221, combinedResult.BetType);
        Assert.Equal(10, combinedResult.Position);
        Assert.Equal(3, combinedResult.Result); // Sum of results
        Assert.Equal(8, combinedResult.TicketCount); // Sum of ticket counts
        Assert.Equal(3, combinedResult.TicketRunning); // Sum of running tickets
        Assert.Equal(3, combinedResult.TicketMarked); // Sum of marked tickets
    }

    [Fact]
    public async Task GetFastMarketTicketDetail_WithNullResults_HandlesGracefully()
    {
        // Arrange
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((new List<TicketCounts>(), new List<FastMarketResultDetail>()));

        // Act
        var method = typeof(GetFastMarketResultsHandler).GetMethod(
            "GetFastMarketTicketDetail",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = await (Task<List<FastMarketResultDetail>>)method.Invoke(sut, [123]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FilterResultsByWaterBreakAndStatus_WithAllStatuses_FiltersCorrectly()
    {
        // Arrange
        var session = new SpecialSession
        {
            HomeValue = "10",
            AwayValue = "15"
        };

        var combinedResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 221, Position = 10, ResultStatus = 0 }, // Runnable
            new FastMarketResultDetail { BetType = 222, Position = 11, ResultStatus = 1 }, // Runnable
            new FastMarketResultDetail { BetType = 223, Position = 12, ResultStatus = 2 }, // Runnable
            new FastMarketResultDetail { BetType = 224, Position = 13, ResultStatus = 3 }, // Runnable
            new FastMarketResultDetail { BetType = 225, Position = 14, ResultStatus = 4 }, // Runnable (CompletedAutoResult)
            new FastMarketResultDetail { BetType = 226, Position = 15, ResultStatus = 5 }, // Runnable (CompletedManualResult)
            new FastMarketResultDetail { BetType = 227, Position = 16, ResultStatus = 6 }, // Not runnable
            new FastMarketResultDetail { BetType = 228, Position = 17, ResultStatus = 7 }  // Not runnable
        };

        var filteredResults = combinedResults.Where(r => new[] { 0, 1, 2, 3, 4, 5 }.Contains(r.ResultStatus)).ToList();

        filterWaterBreakService.ApplyFilterByWaterBreak(Arg.Any<IEnumerable<FastMarketResultDetail>>(), session)
            .Returns(filteredResults);

        // Act
        var result = typeof(GetFastMarketResultsHandler)
            .GetMethod("FilterResultsByWaterBreakAndStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(sut, new object[] { combinedResults, session }) as List<FastMarketResultDetail>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Count);
        Assert.DoesNotContain(result, r => r.ResultStatus == 6);
        Assert.DoesNotContain(result, r => r.ResultStatus == 7);
    }

    #endregion
}
