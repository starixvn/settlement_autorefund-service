using System.Threading.Tasks;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using NSubstitute;
using Xunit;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Constants;

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
        var match = new Match { MatchId = 123 };
        var betTypesRoot = new BetTypesRoot();
        var betTypesXml = "<BetTypesRoot />";
        var validationResult = new ResultMessage { ErrorCode = 0 };

        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns(betTypesXml);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(validationResult);

        var handler = new PushFastMarketResultHandler(scaleOutSettleService, getFastMarketResultsHandler, xmlConvertService);

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
        var match = new Match { MatchId = 123 };
        var betTypesRoot = new BetTypesRoot();
        var betTypesXml = "<BetTypesRoot />";
        var validationResult = new ResultMessage { ErrorCode = 1 };

        getFastMarketResultsHandler.Handle(match).Returns(betTypesRoot);
        xmlConvertService.Serialize(betTypesRoot).Returns(betTypesXml);
        scaleOutSettleService.ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>())
            .Returns(validationResult);

        var handler = new PushFastMarketResultHandler(scaleOutSettleService, getFastMarketResultsHandler, xmlConvertService);

        // Act
        await handler.Handle(match);

        // Assert
        await scaleOutSettleService.Received(1).ValidateEventAndTicketFastMarket(Arg.Any<EventTicketFastMarketRequest>());
        await scaleOutSettleService.DidNotReceive().UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>());
    }
}