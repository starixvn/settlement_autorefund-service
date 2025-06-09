using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResultDetails;
using AutoEarlyRefund.Features.GetSrcResources;
using AutoEarlyRefund.Models;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.RefundConsumerHandler;

public class RefundConsumerHandlerTests : TestBase
{
    private readonly IGetRefundMatchInfoHandler getRefundMatchInfoHandler;
    private readonly AutoEarlyRefund.Features.RefundConsumerHandler.RefundConsumerHandler handler;

    public RefundConsumerHandlerTests()
    {
        var getResourcesConditionsHandler1 = Substitute.For<IGetResourcesConditionsHandler>();
        var resourcesInfoHandler1 = Substitute.For<IGetSrcResourcesHandler>();
        var xmlConvertService1 = Substitute.For<XmlConvertService>();
        getRefundMatchInfoHandler = Substitute.For<IGetRefundMatchInfoHandler>();
        var getRefundResultsHandler1 = Substitute.For<IGetResultDetailsHandler>();
        var getAutoRefundResultsHandler1 = Substitute.For<IGetAutoRefundResultsHandler>();

        handler = new AutoEarlyRefund.Features.RefundConsumerHandler.RefundConsumerHandler(
            getResourcesConditionsHandler1,
            resourcesInfoHandler1,
            xmlConvertService1,
            getRefundMatchInfoHandler,
            getRefundResultsHandler1,
            getAutoRefundResultsHandler1);
    }

    [Fact]
    public async Task Handle_WhenTriggerResourcesEmpty_ShouldReturnEmptyResult()
    {
        // Arrange
        const int matchId = 123;
        var triggerResources = new List<TargetResourceTrigger>();

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_WhenNoRelatedMatchItems_ShouldReturnEmptyResult()
    {
        // Arrange
        const int matchId = 123;
        var triggerResources = new List<TargetResourceTrigger>
        {
            new() { ChildMatchType = 1, TargetResourceChildMatchType = 2 }
        };

        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(new List<RelatedMatchItem>());

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_WhenNoValidResults_ShouldReturnEmptyResult()
    {
        // Arrange
        const int matchId = 123;
        var triggerResources = new List<TargetResourceTrigger>
        {
            new() { ChildMatchType = 1, TargetResourceChildMatchType = 2 }
        };

        var relatedMatchItems = new List<RelatedMatchItem>
        {
            new() { TargetMatchId = 456, ChildMatchType = 1 }
        };

        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }
}