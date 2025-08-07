using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services;

public class FastMarketValidationServiceTests
{
    private readonly AutoRefundResultService refundResultService = Substitute.For<AutoRefundResultService>();
    private readonly ScaleOutSettleService scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
    private readonly ICheckMarkedTicketHandler checkMarkedTicketHandler = Substitute.For<ICheckMarkedTicketHandler>();
    private readonly FastMarketValidationService sut;

    public FastMarketValidationServiceTests()
    {
        sut = new FastMarketValidationService(refundResultService, scaleOutSettleService, checkMarkedTicketHandler);
    }

    [Fact]
    public async Task ValidRunAutoRefund_NoExistingRefunds_ReturnsAllSessionsValid()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                ResourceId = "A",
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail { BetType = 1, Position = 1, SourceId = "A" }
                }
            }
        };
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>())
            .Returns(new List<AutoRefundResult>());

        // Act
        var result = await sut.ValidRunAutoRefund(match, sessions);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(sessions, result.Sessions);
        Assert.Empty(result.InvalidSessions);
    }

    [Fact]
    public async Task ValidRunAutoRefund_ExistingRefunds_ReturnsValidSessionsFiltered()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                ResourceId = "A",
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail { BetType = 1, Position = 1, SourceId = "A" }
                }
            },
            new SpecialSession
            {
                ResourceId = "B",
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail { BetType = 2, Position = 2, SourceId = "B" }
                }
            }
        };
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>())
            .Returns(new List<AutoRefundResult>
            {
                new AutoRefundResult { ResourceId = "1", OtherInfo = "1" }
            });

        // Act
        var result = await sut.ValidRunAutoRefund(match, sessions);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Sessions);
        Assert.Equal("B", result.Sessions[0].ResourceId);
    }

    [Fact]
    public async Task ValidateEventTicket_ErrorCode_ReturnsInvalid()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var xml = "<xml/>";
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 2, ErrorMessage = "fail" });

        // Act
        var (isValid, errorMsg) = await sut.ValidateEventTicket(match, xml);

        // Assert
        Assert.False(isValid);
        Assert.Equal("fail", errorMsg);
    }

    [Fact]
    public async Task ValidateEventTicket_NoErrorCode_ReturnsValid()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var xml = "<xml/>";
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 0 });

        // Act
        var (isValid, errorMsg) = await sut.ValidateEventTicket(match, xml);

        // Assert
        Assert.True(isValid);
        Assert.Equal(string.Empty, errorMsg);
    }

    [Fact]
    public async Task HasMarkedTickets_ReturnsHandlerResult()
    {
        // Arrange
        checkMarkedTicketHandler.Handle(1, "xml").Returns(true);

        // Act
        var result = await sut.HasMarkedTickets(1, "xml");

        // Assert
        Assert.True(result);
        await checkMarkedTicketHandler.Received(1).Handle(1, "xml");
    }

    [Fact]
    public void ValidateSessionsWithCompletedResults_NoCompleted_ReturnsAllValid()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                ResourceId = "A",
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail { ResultStatus = 0 }
                }
            }
        };

        // Act
        var result = sut.ValidateSessionsWithCompletedResults(match, sessions);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(sessions, result.Sessions);
        Assert.Empty(result.InvalidSessions);
    }

    [Fact]
    public void ValidateSessionsWithCompletedResults_HasCompleted_ReturnsFilteredAndError()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var completedStatus = Status.CompletedManualResult;
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                ResourceId = "A",
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail { ResultStatus = completedStatus }
                }
            },
            new SpecialSession
            {
                ResourceId = "B",
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail { ResultStatus = 0 }
                }
            }
        };

        // Act
        var result = sut.ValidateSessionsWithCompletedResults(match, sessions);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Sessions);
        Assert.Equal("B", result.Sessions[0].ResourceId);
        Assert.Single(result.InvalidSessions);
        Assert.Contains("Auto Refund failed", result.ErrorMessage);
    }
}
