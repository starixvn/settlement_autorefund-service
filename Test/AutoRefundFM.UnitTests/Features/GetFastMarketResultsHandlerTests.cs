using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class GetFastMarketResultsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectBetTypesRoot()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = new AppSettingsKey { SiteDbServer = ["db1"] };
        var options = Substitute.For<IOptions<AppSettingsKey>>();
        options.Value.Returns(appSettingsKey);

        var match = new Match
        {
            MatchId = 1,
            SpecialSessions =
            [
                new SpecialSession { StartMinute = 10, EndMinute = 15 },
                new SpecialSession { StartMinute = 20, EndMinute = 25 }
            ]
        };

        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new() {
                BetType = FastMarketBetType.NextOneMinute,
                Position = 12,
                Result = 1,
                ResultItem = new() { None = true }
            },
            new() {
                BetType = FastMarketBetType.TimeMachineHdp,
                Position = 22,
                Result = 2,
                ResultItem = new() { Goal = true }
            }
        };

        var ticketResults = new List<FastMarketResultDetail>
        {
            new() {
                BetType = FastMarketBetType.NextOneMinute,
                Position = 21,
                Result = 3,
                ResultItem = new() { None = true, Goal = true }
            }
        };

        var refundResultService = Substitute.For<AutoRefundResultService>();

        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, ticketResults));

        // Mock refundResultService.GetResults to filter out the item with BetType = NextOneMinute, Position = 12
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>())
            .Returns(Task.FromResult(new List<AutoRefundResult>
            {
                new AutoRefundResult { ResourceId = FastMarketBetType.NextOneMinute.ToString(), OtherInfo = "12" }
            }));

        var handler = new GetFastMarketResultsHandler(repository, options, refundResultService);

        // Act
        var result = await handler.Handle(match);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BetType);
        Assert.Contains(result.BetType, b => b.Id == FastMarketBetType.NextOneMinute);
        var nextOneMinute = result.BetType.First(b => b.Id == FastMarketBetType.NextOneMinute);
        // Should NOT contain the filtered-out item (Position = 12)
        Assert.DoesNotContain(nextOneMinute.Items, i => i.Position == 12 && i.Result == "1");
        // Should still contain the unfiltered item (Position = 21)
        Assert.Contains(nextOneMinute.Items, i => i.Position == 21 && i.Result == "3");
    }

    [Fact]
    public async Task Handle_NoFilteringNeeded_ReturnsAllItems()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = new AppSettingsKey { SiteDbServer = new List<string> { "db1" } };
        var options = Substitute.For<IOptions<AppSettingsKey>>();
        options.Value.Returns(appSettingsKey);

        var match = new Match
        {
            MatchId = 2,
            SpecialSessions =
            [
                new() { StartMinute = 10, EndMinute = 15 }
            ]
        };

        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new() { BetType = FastMarketBetType.NextOneMinute, Position = 12, Result = 1, ResultItem = new() { None = true } },
            new() { BetType = FastMarketBetType.NextOneMinute, Position = 13, Result = 2, ResultItem = new() { Goal = true } }
        };
        var ticketResults = new List<FastMarketResultDetail>();
        var refundResultService = Substitute.For<AutoRefundResultService>();
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, ticketResults));
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>())
            .Returns(Task.FromResult(new List<AutoRefundResult>()));
        var handler = new GetFastMarketResultsHandler(repository, options, refundResultService);

        // Act
        var result = await handler.Handle(match);

        // Assert
        var nextOneMinute = result.BetType.First(b => b.Id == FastMarketBetType.NextOneMinute);
        Assert.Contains(nextOneMinute.Items, i => i.Position == 12 && i.Result == "1");
        Assert.Contains(nextOneMinute.Items, i => i.Position == 13 && i.Result == "2");
    }

    [Fact]
    public async Task Handle_AllResultsFiltered_ReturnsNoItemsForBetType()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = new AppSettingsKey { SiteDbServer = new List<string> { "db1" } };
        var options = Substitute.For<IOptions<AppSettingsKey>>();
        options.Value.Returns(appSettingsKey);
        var match = new Match
        {
            MatchId = 3,
            SpecialSessions = [new() { StartMinute = 10, EndMinute = 15 }]
        };
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new()
        };
        var ticketResults = new List<FastMarketResultDetail>();
        var refundResultService = Substitute.For<AutoRefundResultService>();
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, ticketResults));
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>())
            .Returns(Task.FromResult(new List<AutoRefundResult>
            {
                new() { ResourceId = FastMarketBetType.NextOneMinute.ToString(), OtherInfo = "12" },
                new() { ResourceId = FastMarketBetType.NextOneMinute.ToString(), OtherInfo = "13" }
            }));
        var handler = new GetFastMarketResultsHandler(repository, options, refundResultService);

        // Act
        var result = await handler.Handle(match);

        // Assert
        var nextOneMinute = result.BetType.FirstOrDefault(b => b.Id == FastMarketBetType.NextOneMinute);
        Assert.True(nextOneMinute == null || !nextOneMinute.Items.Any());
    }


    [Fact]
    public async Task Handle_EmptyFastMarketAndTicketResults_ReturnsEmpty()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = new AppSettingsKey { SiteDbServer = new List<string> { "db1" } };
        var options = Substitute.For<IOptions<AppSettingsKey>>();
        options.Value.Returns(appSettingsKey);
        var match = new Match
        {
            MatchId = 6,
            SpecialSessions = [new() { StartMinute = 10, EndMinute = 15 }]
        };
        var fastMarketResults = new List<FastMarketResultDetail>();
        var ticketResults = new List<FastMarketResultDetail>();
        var refundResultService = Substitute.For<AutoRefundResultService>();
        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, ticketResults));
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>())
            .Returns(Task.FromResult(new List<AutoRefundResult>()));
        var handler = new GetFastMarketResultsHandler(repository, options, refundResultService);

        // Act
        var result = await handler.Handle(match);

        // Assert
        Assert.Empty(result.BetType);
    }
}