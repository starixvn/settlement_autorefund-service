using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class PushFastMarketResultHandlerTests
{
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly IGetFastMarketResultsHandler getFastMarketResultsHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly SettlementLogService logService;
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly AutoRefundResultService refundResultService;
    private readonly IUpdateWaterBreakHandler updateWaterBreakHandler;
    private readonly Match match;

    public PushFastMarketResultHandlerTests()
    {
        scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        getFastMarketResultsHandler = Substitute.For<IGetFastMarketResultsHandler>();
        xmlConvertService = Substitute.For<XmlConvertService>();
        logService = Substitute.For<SettlementLogService>();
        repository = Substitute.For<IDynamicRepository>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        refundResultService = Substitute.For<AutoRefundResultService>();
        updateWaterBreakHandler = Substitute.For<IUpdateWaterBreakHandler>();
        match = new Match { MatchId = 123 };
    }

    [Fact]
    public async Task Handle_BetTypeRootEmpty_CallsUpdateWaterBreakAndReturnsEarly()
    {
        // Arrange
        var betTypesRoot = new BetTypesRoot { BetType = new List<BetTypeElement>() };

        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);

        var handler = new PushFastMarketResultHandler(
            scaleOutSettleService,
            getFastMarketResultsHandler,
            xmlConvertService,
            logService,
            repository,
            appSettingsKey,
            refundResultService,
            updateWaterBreakHandler);

        // Act
        await handler.Handle(match);

        // Assert
        await updateWaterBreakHandler.Received(1).Handle(match);
        await scaleOutSettleService.DidNotReceive().ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>());
        await scaleOutSettleService.DidNotReceive().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
    }


    [Fact]
    public async Task Handle_ValidationFails_DoesNotCallUpdateOrLogMethods()
    {
        // Arrange
        var betTypesRoot = new BetTypesRoot { BetType = new List<BetTypeElement> { new() { Id = FastMarketBetType.NextOneMinute, Items = new List<ItemElement> { new() { Position = 1, Result = "1" } } } } };
        var betTypesXml = "<BetTypesRoot />";
        var validationResult = new ResultMessage { ErrorCode = 1 };

        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns(betTypesXml);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>()).Returns(validationResult);

        var handler = new PushFastMarketResultHandler(
            scaleOutSettleService,
            getFastMarketResultsHandler,
            xmlConvertService,
            logService,
            repository,
            appSettingsKey,
            refundResultService,
            updateWaterBreakHandler);

        // Act
        await handler.Handle(match);

        // Assert
        await scaleOutSettleService.Received(1).ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>());
        await scaleOutSettleService.DidNotReceive().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
        await logService.DidNotReceive().WriteUserLogSettle(Arg.Any<int>(), Arg.Any<string>());
        await logService.DidNotReceive().WriteSettlementLog(Arg.Any<InsertSettlementLogCriteria>());
        await refundResultService.DidNotReceive().InsertOrUpdateResults(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
        await updateWaterBreakHandler.DidNotReceive().Handle(match);
    }
}