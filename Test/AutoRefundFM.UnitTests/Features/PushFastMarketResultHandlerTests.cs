using AutoRefund.Core.Models;
using AutoRefund.Services;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class PushFastMarketResultHandlerTests
{
    [Fact]
    public async Task Handle_ValidatesAndUpdatesMatchInfo_WhenValidationPasses()
    {
        // Arrange
        var scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        var getFastMarketResultsHandler = Substitute.For<IGetFastMarketResultsHandler>();
        var xmlConvertService = Substitute.For<XmlConvertService>();
        var logService = Substitute.For<LogService>();
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        var match = new Match { MatchId = 123 };
        var betTypesRoot = new BetTypesRoot();
        var betTypesXml = "<BetTypesRoot />";
        var validationResult = new ResultMessage { ErrorCode = 0 };

        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns(betTypesXml);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(validationResult);

        var handler = new PushFastMarketResultHandler(
            scaleOutSettleService,
            getFastMarketResultsHandler,
            xmlConvertService,
            logService,
            repository,
            appSettingsKey);

        // Act
        await handler.Handle(match);

        // Assert
        await scaleOutSettleService.Received(1).ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>());
        await scaleOutSettleService.Received(1).UpdateMatchInfo(Arg.Is<UpdateMatchInfoRequest>(r => r.Mode == Mode.FastMarket));
    }

    [Fact]
    public async Task Handle_DoesNotUpdateMatchInfo_WhenValidationFails()
    {
        // Arrange
        var scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        var getFastMarketResultsHandler = Substitute.For<IGetFastMarketResultsHandler>();
        var xmlConvertService = Substitute.For<XmlConvertService>();
        var logService = Substitute.For<LogService>();
        var repository = Substitute.For<IDynamicRepository>();
        var appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        var match = new Match { MatchId = 123 };
        var betTypesRoot = new BetTypesRoot();
        var betTypesXml = "<BetTypesRoot />";
        var validationResult = new ResultMessage { ErrorCode = 1 };

        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns(betTypesXml);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(validationResult);

        var handler = new PushFastMarketResultHandler(
            scaleOutSettleService,
            getFastMarketResultsHandler,
            xmlConvertService,
            logService,
            repository,
            appSettingsKey);

        // Act
        await handler.Handle(match);

        // Assert
        await scaleOutSettleService.Received(1).ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>());
        await scaleOutSettleService.DidNotReceive().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
    }
}