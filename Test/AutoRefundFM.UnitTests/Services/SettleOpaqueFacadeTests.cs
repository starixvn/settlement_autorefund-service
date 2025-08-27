using AutoRefund.Core.Criteria;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services;

public class SettleOpaqueFacadeImpTests
{
    private readonly IGetFastMarketResultsHandler getFastMarketResultsHandler = Substitute.For<IGetFastMarketResultsHandler>();
    private readonly IPushFastMarketResultHandler pushFastMarketResultHandler = Substitute.For<IPushFastMarketResultHandler>();
    private readonly ICheckMarkedTicketHandler checkMarkedTicketHandler = Substitute.For<ICheckMarkedTicketHandler>();
    private readonly IMatchPoolService matchPoolService = Substitute.For<IMatchPoolService>();
    private readonly SettlementLogService settlementLogService = Substitute.For<SettlementLogService>();
    private readonly IFastMarketValidationService fastMarketValidationService = Substitute.For<IFastMarketValidationService>();
    private readonly IFmRefundResultService fmRefundResultService = Substitute.For<IFmRefundResultService>();
    private readonly IFastMarketDataTransformationService dataTransformationService = Substitute.For<IFastMarketDataTransformationService>();

    private readonly Match match = new() { MatchId = 1, SpecialSessions = new List<SpecialSession>() };

    private SettleOpaqueFacadeImp CreateSut() => new(
        getFastMarketResultsHandler,
        pushFastMarketResultHandler,
        checkMarkedTicketHandler,
        matchPoolService,
        settlementLogService,
        fastMarketValidationService,
        fmRefundResultService,
        dataTransformationService);

    [Fact]
    public async Task Refund_CompletedResult_InvalidSessions_CallsHandleMatchError()
    {
        // Arrange
        var sessionResults = new List<SpecialSession> { new SpecialSession() };
        getFastMarketResultsHandler.Handle(match).Returns(sessionResults);
        fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults)
            .Returns(new ValidateResult(false, "err", new List<SpecialSession>(), new List<SpecialSession> { new SpecialSession() }));

        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await fmRefundResultService.Received(1).AddOrUpdateSessionResult(match.MatchId, Arg.Any<List<SpecialSession>>(), 3, "err");
        await settlementLogService.Received(1).WriteUserLogSettle(Arg.Any<UserLogCriteria>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_CompletedResult_ValidSessionsEmpty_ReturnsEarly()
    {
        // Arrange
        var sessionResults = new List<SpecialSession> { new SpecialSession() };
        getFastMarketResultsHandler.Handle(match).Returns(sessionResults);
        fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults)
            .Returns(new ValidateResult(true, "", new List<SpecialSession>(), new List<SpecialSession>()));

        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await fmRefundResultService.Received(1).AddOrUpdateSessionResult(match.MatchId, match.SpecialSessions, 1);
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_ValidRunAutoRefund_InvalidSessions_CallsAddOrUpdateSessionResult()
    {
        // Arrange
        var sessionResults = new List<SpecialSession> { new SpecialSession() };
        getFastMarketResultsHandler.Handle(match).Returns(sessionResults);
        fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        fastMarketValidationService.ValidRunAutoRefund(match, sessionResults)
            .Returns(new ValidateResult(false, "", new List<SpecialSession>(), new List<SpecialSession> { new SpecialSession() }));

        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await fmRefundResultService.Received(1).AddOrUpdateSessionResult(match.MatchId, Arg.Any<List<SpecialSession>>(), 2);
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_ValidationResultInvalid_CallsHandleMatchError()
    {
        // Arrange
        var sessionResults = new List<SpecialSession> { new SpecialSession() };
        getFastMarketResultsHandler.Handle(match).Returns(sessionResults);
        fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        fastMarketValidationService.ValidRunAutoRefund(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        dataTransformationService.CreateBetTypesRoot(Arg.Any<List<SpecialSession>>()).Returns((new BetTypesRoot(), "<xml>"));
        fastMarketValidationService.ValidateEventTicket(match, "<xml>").Returns(Task.FromResult((false, "validation error")));

        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await fmRefundResultService.Received(1).AddOrUpdateSessionResult(match.MatchId, Arg.Any<List<SpecialSession>>(), 3, "validation error");
        await settlementLogService.Received(1).WriteUserLogSettle(Arg.Any<UserLogCriteria>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_HasMarkedTickets_CallsHandleMatchError()
    {
        // Arrange
        var sessionResults = new List<SpecialSession> { new SpecialSession() };
        getFastMarketResultsHandler.Handle(match).Returns(sessionResults);
        fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        fastMarketValidationService.ValidRunAutoRefund(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        dataTransformationService.CreateBetTypesRoot(Arg.Any<List<SpecialSession>>()).Returns((new BetTypesRoot(), "<xml>"));
        fastMarketValidationService.ValidateEventTicket(match, "<xml>").Returns(Task.FromResult((true, "")));
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(Task.FromResult(true));

        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await fmRefundResultService.Received(1).AddOrUpdateSessionResult(match.MatchId, Arg.Any<List<SpecialSession>>(), 3, Arg.Any<string>());
        await settlementLogService.Received(1).WriteUserLogSettle(Arg.Any<UserLogCriteria>());
        matchPoolService.Received(1).RemoveMatch(match);
    }

    [Fact]
    public async Task Refund_SuccessfulFlow_CallsProcessRefund()
    {
        // Arrange
        var sessionResults = new List<SpecialSession> { new SpecialSession() };
        getFastMarketResultsHandler.Handle(match).Returns(sessionResults);
        fastMarketValidationService.ValidateSessionsWithCompletedResults(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        fastMarketValidationService.ValidRunAutoRefund(match, sessionResults)
            .Returns(new ValidateResult(true, "", sessionResults, new List<SpecialSession>()));
        dataTransformationService.CreateBetTypesRoot(Arg.Any<List<SpecialSession>>()).Returns((new BetTypesRoot(), "<xml>"));
        fastMarketValidationService.ValidateEventTicket(match, "<xml>").Returns(Task.FromResult((true, "")));
        checkMarkedTicketHandler.Handle(match.MatchId, "<xml>").Returns(Task.FromResult(false));

        var sut = CreateSut();

        // Act
        await sut.Refund(match);

        // Assert
        await pushFastMarketResultHandler.Received(1).Handle(Arg.Any<Match>(), Arg.Any<PushFastMarketResultCriteria>());
        matchPoolService.Received(1).RemoveMatch(match);
    }
}
