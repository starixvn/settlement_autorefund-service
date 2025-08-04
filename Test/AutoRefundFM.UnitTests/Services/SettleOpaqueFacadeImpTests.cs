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
    }

    [Fact]
    public async Task Refund_ValidationFails_CallsHandleInValidRefundResultAndReturns()
    {
        // Arrange
        var betTypesRoot = new BetTypesRoot { BetType = new List<BetTypeElement> { new() { Id = 1, Items = new List<ItemElement> { new() { Position = 1, Result = "1" } } } } };
        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns("<xml>");
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 1, ErrorMessage = "Validation failed" });
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(match.MatchId, 3, Arg.Any<string>());
        await pushFastMarketResultHandler.DidNotReceive().Handle(
            Arg.Any<Match>(), Arg.Any<PushFastMarketResultCriteria>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Refund_MarkedTicketFound_CallsHandleInValidRefundResultAndReturns()
    {
        // Arrange
        var betTypesRoot = new BetTypesRoot { BetType = new List<BetTypeElement> { new() { Id = 1, Items = new List<ItemElement> { new() { Position = 1, Result = "1" } } } } };
        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns("<xml>");
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
    }

    [Fact]
    public async Task Refund_SuccessfulFlow_CallsPushFastMarketResultHandler()
    {
        // Arrange
        var betTypesRoot = new BetTypesRoot { BetType = new List<BetTypeElement> { new() { Id = 1, Items = new List<ItemElement> { new() { Position = 1, Result = "1" } } } } };
        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns("<xml>");
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(new ResultMessage { ErrorCode = 0 });
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(false);
        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await pushFastMarketResultHandler.Received(1).Handle(
            match, Arg.Any<PushFastMarketResultCriteria>(), Arg.Any<string>());
    }
}