using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class SettleOpaqueFacadeImpTests
{
    private readonly IGetFastMarketResultsHandler getFastMarketResultsHandler = Substitute.For<IGetFastMarketResultsHandler>();
    private readonly IUpdateWaterBreakHandler updateWaterBreakHandler = Substitute.For<IUpdateWaterBreakHandler>();
    private readonly ScaleOutSettleService scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
    private readonly XmlConvertService xmlConvertService = Substitute.For<XmlConvertService>();
    private readonly IPushFastMarketResultHandler pushFastMarketResultHandler = Substitute.For<IPushFastMarketResultHandler>();
    private readonly AutoRefundResultService refundResultService = Substitute.For<AutoRefundResultService>();
    private readonly ICheckMarkedTicketHandler checkMarkedTicketHandler = Substitute.For<ICheckMarkedTicketHandler>();
    private readonly IMatchPoolService matchPoolService = Substitute.For<IMatchPoolService>();
    private readonly Match match = new() { MatchId = 123 };

    private SettleOpaqueFacadeImp CreateSut() => new(
        getFastMarketResultsHandler,
        updateWaterBreakHandler,
        scaleOutSettleService,
        xmlConvertService,
        pushFastMarketResultHandler,
        refundResultService,
        checkMarkedTicketHandler,
        matchPoolService);

    #region Refund Method Tests

    [Fact]
    public async Task Refund_BetTypeRootEmpty_CallsUpdateWaterBreakAndReturnsEarly()
    {
        // Arrange
        getFastMarketResultsHandler.Handle(match).Returns(new BetTypesRoot { BetType = new List<BetTypeElement>() });
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await updateWaterBreakHandler.Received(1).Handle(match);
        await pushFastMarketResultHandler.DidNotReceive().Handle(
            Arg.Any<Match>(), Arg.Any<PushFastMarketResultCriteria>(), Arg.Any<string>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_ValidationFails_CallsHandleInValidRefundResultAndReturns()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 1, ErrorMessage = "Validation failed" });
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(match.MatchId, 3, Arg.Any<string>());
        await pushFastMarketResultHandler.DidNotReceive().Handle(
            Arg.Any<Match>(), Arg.Any<PushFastMarketResultCriteria>(), Arg.Any<string>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_MarkedTicketFound_CallsHandleInValidRefundResultAndReturns()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(true);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(match.MatchId, 3, Arg.Any<string>());
        await pushFastMarketResultHandler.DidNotReceive().Handle(
            Arg.Any<Match>(), Arg.Any<PushFastMarketResultCriteria>(), Arg.Any<string>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_SuccessfulFlow_CallsPushFastMarketResultHandler()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(false);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await pushFastMarketResultHandler.Received(1).Handle(
            match, Arg.Any<PushFastMarketResultCriteria>(), Arg.Any<string>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    #endregion

    #region Private Method Tests (Testing through public interface)

    [Fact]
    public async Task Refund_WhenBetTypeCountIsZero_UpdatesWaterBreakAndReturns()
    {
        // Arrange
        var emptyBetTypesRoot = new BetTypesRoot { BetType = new List<BetTypeElement>() };
        getFastMarketResultsHandler.Handle(match).Returns(emptyBetTypesRoot);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await updateWaterBreakHandler.Received(1).Handle(match);
        xmlConvertService.DidNotReceive().Serialize(Arg.Any<object>());
    }

    [Fact]
    public async Task Refund_WhenBetTypeCountIsGreaterThanZero_PreparesRefundData()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(false);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        xmlConvertService.Received(2).Serialize(Arg.Any<object>()); // Once for betTypesRoot, once for resources
        await updateWaterBreakHandler.DidNotReceive().Handle(match);
    }

    [Fact]
    public async Task Refund_ValidationRequest_SetsIsAutoToFalse()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        EventTicketFastMarketRequest capturedRequest = null;
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Do<EventTicketFastMarketRequest>(req => capturedRequest = req))
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(false);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        Assert.False(capturedRequest?.IsAuto);
    }

    [Fact]
    public async Task Refund_ValidationRequest_SetsCorrectMatchIdAndResults()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        EventTicketFastMarketRequest capturedRequest = null;
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Do<EventTicketFastMarketRequest>(req => capturedRequest = req))
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(false);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        Assert.Equal(match.MatchId, capturedRequest?.MatchId);
        Assert.Equal("<xml>", capturedRequest?.Results);
    }

    [Fact]
    public async Task Refund_WithMarkedTickets_PassesCorrectErrorMessage()
    {
        // Arrange
        var betTypesRoot = CreateBetTypesRoot();
        SetupValidBetTypesRoot(betTypesRoot);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(true);
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<resources>");
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(
            match.MatchId,
            3,
            "<resources>");
    }

    #endregion


    #region Helper Methods

    private BetTypesRoot CreateBetTypesRoot()
    {
        return new BetTypesRoot
        {
            BetType = new List<BetTypeElement>
            {
                new()
                {
                    Id = 1,
                    Items = new List<ItemElement>
                    {
                        new() { Position = 1, Result = "1" },
                        new() { Position = 2, Result = "0" }
                    }
                }
            }
        };
    }

    private void SetupValidBetTypesRoot(BetTypesRoot betTypesRoot)
    {
        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns("<xml>");
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<resources>");
    }

    #endregion
}