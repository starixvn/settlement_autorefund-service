using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Models;
using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoRefundFM.UnitTests.Features;

public class GetFastMarketResultsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectBetTypesRoot()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = new AppSettingsKey { SiteDbServer = new List<string> { "db1" } };
        var options = Substitute.For<IOptions<AppSettingsKey>>();
        options.Value.Returns(appSettingsKey);

        var match = new Match
        {
            MatchId = 1,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession { StartMinute = 10, EndMinute = 15 },
                new SpecialSession { StartMinute = 20, EndMinute = 25 }
            }
        };

        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail
            {
                BetType = FastMarketBetType.NextOneMinute,
                Position = 12,
                Result = 1,
                ResultItem = new ResultItem { None = true }
            },
            new FastMarketResultDetail
            {
                BetType = FastMarketBetType.TimeMachineHdp,
                Position = 22,
                Result = 2,
                ResultItem = new ResultItem { Goal = true }
            }
        };

        var ticketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail
            {
                BetType = FastMarketBetType.NextOneMinute,
                Position = 21,
                Result = 3,
                ResultItem = new ResultItem { None = true, Goal = true }
            }
        };

        repository.FetchMultipleAsync<EventCounter, string, FastMarketResultDetail, ResultDetail>(Arg.Any<GetFastMarketResultsCriteria>())
            .Returns((null, null, fastMarketResults, null));
        repository.FetchMultipleAsync<TicketCounts, FastMarketResultDetail>(Arg.Any<GetFastMarketTicketDetailCriteria>())
            .Returns((null, ticketResults));

        var handler = new GetFastMarketResultsHandler(repository, options);

        // Act
        var result = await handler.Handle(match);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BetType);
        Assert.Contains(result.BetType, b => b.Id == FastMarketBetType.NextOneMinute);
        var nextOneMinute = result.BetType.First(b => b.Id == FastMarketBetType.NextOneMinute);
        Assert.Contains(nextOneMinute.Items, i => i.Position == 12 && i.Result == "1");
        Assert.Contains(nextOneMinute.Items, i => i.Position == 21 && i.Result == "3");
    }
}