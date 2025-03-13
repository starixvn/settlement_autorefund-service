using AutoEarlyRefund.Features.CheckConditionsRefund;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features;

public class CheckConditionsRefundHandlerTests
{
    private readonly IValidateHandler validateHandler;
    private readonly IGetResourcesConditionsHandler conditionsHandler; //GetResourcesConditionsHandler
    private readonly IGetSrcResourcesHandler getSrcResourcesHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult;
    private readonly XmlConvertService xmlConvertService;
    private readonly CheckConditionsRefundHandler handler;

    public CheckConditionsRefundHandlerTests()
    {
        validateHandler = Substitute.For<IValidateHandler>();
        conditionsHandler = Substitute.For<IGetResourcesConditionsHandler>();
        getSrcResourcesHandler = Substitute.For<IGetSrcResourcesHandler>();
        insOrUpdAutoPayoutResult = Substitute.For<IInsOrUpdAutoPayoutResultHandler>();
        xmlConvertService = Substitute.For<XmlConvertService>();

        handler = new CheckConditionsRefundHandler(
            validateHandler,
            conditionsHandler,
            getSrcResourcesHandler,
            insOrUpdAutoPayoutResult,
            xmlConvertService);
    }


    [Fact]
    public async Task HandleAsync_ShouldThrowAppException_WhenNoValidResults()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            ResultDetails = new List<ResultDetail>
        {
            new ResultDetail { ResourceId = "1", SportType = 1, ChildMatchType = 1, SourceId = "1", OtherInfo = "{}" }
        }
        };

        var conditions = new List<SourcesConditions>
    {
        new SourcesConditions { ResourceId = "1", RuleName = "Rule1", Resource = null }
    };

        var resources = new List<SourcesConditions>();

        conditionsHandler.GetConditionsAsync(Arg.Any<List<SourcesConditionsItem>>()).Returns(conditions);
        getSrcResourcesHandler.HandleAsync(Arg.Any<int>(), Arg.Any<List<SourcesConditions>>()).Returns(resources);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => handler.HandleAsync(match));
    }
}
