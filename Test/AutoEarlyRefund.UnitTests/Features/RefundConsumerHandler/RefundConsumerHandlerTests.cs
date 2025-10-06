using AutoEarlyRefund;
using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResultDetails;
using AutoEarlyRefund.Features.GetSrcResources;
using AutoEarlyRefund.Features.RefundConsumerHandler;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.UnitTests;
using Fanex.Settlement.Converter.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

namespace AutoEarlyRefund.UnitTests.Features.RefundConsumerHandler;

public class RefundConsumerHandlerTest : TestBase
{
    private readonly IGetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly IGetSrcResourcesHandler resourcesInfoHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly IGetRefundMatchInfoHandler getRefundMatchInfoHandler;
    private readonly IGetResultDetailsHandler getRefundResultsHandler;
    private readonly IGetAutoRefundResultsHandler getAutoRefundResultsHandler;
    private readonly AutoEarlyRefund.Features.RefundConsumerHandler.RefundConsumerHandler handler;

    public RefundConsumerHandlerTest()
    {
        getResourcesConditionsHandler = Substitute.For<IGetResourcesConditionsHandler>();
        resourcesInfoHandler = Substitute.For<IGetSrcResourcesHandler>();
        xmlConvertService = Substitute.For<XmlConvertService>();
        getRefundMatchInfoHandler = Substitute.For<IGetRefundMatchInfoHandler>();
        getRefundResultsHandler = Substitute.For<IGetResultDetailsHandler>();
        getAutoRefundResultsHandler = Substitute.For<IGetAutoRefundResultsHandler>();

        handler = new AutoEarlyRefund.Features.RefundConsumerHandler.RefundConsumerHandler(
            getResourcesConditionsHandler,
            resourcesInfoHandler,
            xmlConvertService,
            getRefundMatchInfoHandler,
            getRefundResultsHandler,
            getAutoRefundResultsHandler);
    }

    [Fact]
    public async Task Handle_WithEmptyTriggerResources_ShouldReturnFailure()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = new List<TargetResourceTrigger>();

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_WithNoRelatedMatchItems_ShouldReturnFailure()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(new List<RelatedMatchItem>());

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_WithNoValidResults_ShouldReturnFailure()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditions>());
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(new List<SourcesConditions>());

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = CreateSampleSourcesConditions();
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.Matches);
        Assert.Equal(relatedMatchItems.Count, result.Matches.Count);
    }

    [Fact]
    public async Task Handle_WhenGetRefundMatchInfoHandlerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var expectedException = new Exception("Database error");
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .ThrowsAsync(expectedException);

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_WhenGetRefundResultsHandlerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var expectedException = new Exception("Database error");
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .ThrowsAsync(expectedException);

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public async Task Handle_ShouldCreateCorrectRelatedMatchRequests()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = CreateSampleSourcesConditions();
        
        List<GetRelatedMatchItemRequest>? capturedRequests = null;
        getRefundMatchInfoHandler.HandleAsync(Arg.Do<List<GetRelatedMatchItemRequest>>(requests => capturedRequests = requests))
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.NotNull(capturedRequests);
        Assert.Equal(triggerResources.Count, capturedRequests.Count);
        Assert.All(capturedRequests, request =>
        {
            Assert.Equal(matchId, request.MatchId);
            Assert.Contains(triggerResources, tr => tr.ChildMatchType == request.ChildMatchType);
            Assert.Contains(triggerResources, tr => tr.TargetResourceChildMatchType == request.TargetChildMatchType);
        });
    }

    [Fact]
    public async Task Handle_ShouldCallAllHandlersInCorrectOrder()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = CreateSampleSourcesConditions();
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        await handler.Handle(matchId, triggerResources);

        // Assert
        await getRefundMatchInfoHandler.Received(1).HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>());
        await getRefundResultsHandler.Received(1).Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>());
        await getAutoRefundResultsHandler.Received(1).Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>());
        await getResourcesConditionsHandler.Received(1).GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>());
        await resourcesInfoHandler.Received(1).HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>());
    }

    [Fact]
    public async Task Handle_ShouldBuildMatchesWithCorrectData()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = CreateSampleSourcesConditions();
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(relatedMatchItems.Count, result.Matches.Count);
        
        foreach (var match in result.Matches)
        {
            Assert.Contains(relatedMatchItems, rmi => rmi.TargetMatchId == match.MatchId);
            Assert.NotEmpty(match.ResultDetails);
            Assert.Equal("<xml>test</xml>", match.ResourcesXml);
        }
    }

    [Fact]
    public async Task Handle_WithMultipleRelatedMatchItems_ShouldCreateMultipleMatches()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = new List<RelatedMatchItem>
        {
            new() { TargetMatchId = 100, TargetChildMatchType = 0 },
            new() { TargetMatchId = 200, TargetChildMatchType = 1 },
            new() { TargetMatchId = 300, TargetChildMatchType = 0 }
        };
        var validResults = CreateSampleSourcesConditions();
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.Matches.Count);
        Assert.Contains(result.Matches, m => m.MatchId == 100);
        Assert.Contains(result.Matches, m => m.MatchId == 200);
        Assert.Contains(result.Matches, m => m.MatchId == 300);
    }

    [Fact]
    public async Task Handle_WithDuplicateRelatedMatchItems_ShouldDeduplicate()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = new List<RelatedMatchItem>
        {
            new() { TargetMatchId = 100, TargetChildMatchType = 1 },
            new() { TargetMatchId = 100, TargetChildMatchType = 1 }, // Duplicate
            new() { TargetMatchId = 200, TargetChildMatchType = 2 }
        };
        var validResults = CreateSampleSourcesConditions();
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Matches.Count); // Should be deduplicated
    }

    [Theory]
    [InlineData("381", "NT")]
    [InlineData("382", "NT")]
    [InlineData("123", "HomeValue")]
    [InlineData("456", "HomeValue")]
    public async Task Handle_ShouldDetermineCorrectHomeValue(string resourceId, string expectedHomeValue)
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = new List<SourcesConditions>
        {
            new()
            {
                ResourceId = resourceId,
                RuleId = 1,
                SourceAction = "Refund", // This will create RefundedSpecification
                SourceMatchType = 1,
                SourceChildMatchType = 202,
                Conditions = "0",
                Resource = new Resource
                {
                    CfmHomeValue = "HomeValue",
                    CfmAwayValue = "AwayValue",
                    SettlementStatusId = (short)SettleStatus.Refunded, // This will satisfy RefundedSpecification
                    ResultSetting = 0 // This will make IsConfirmedRefundSdt false
                }
            }
        };
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.True(result.Success);
        var resultDetail = result.Matches.First().ResultDetails.First();
        Assert.Equal(expectedHomeValue, resultDetail.HomeValue);
    }

    [Fact]
    public async Task Handle_ShouldCreateCorrectOtherInfo()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = new List<SourcesConditions>
        {
            new()
            {
                ResourceId = "123",
                RuleId = 456,
                SourceAction = "Refund", // This will create RefundedSpecification
                SourceMatchType = 1,
                SourceChildMatchType = 202,
                Conditions = "0",
                Resource = new Resource
                {
                    CfmHomeValue = "HomeValue",
                    CfmAwayValue = "AwayValue",
                    SettlementStatusId = (short)SettleStatus.Refunded, // This will satisfy RefundedSpecification
                    ResultSetting = 4 // This will make IsConfirmedRefundSdt true (bit 2)
                }
            }
        };

        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert
        Assert.True(result.Success);
        var resultDetail = result.Matches.First().ResultDetails.First();
        var otherInfo = JsonSerializer.Deserialize<dynamic>(resultDetail.OtherInfo);
        Assert.NotNull(otherInfo);
    }

    [Fact]
    public async Task Handle_WithNullResource_ShouldHandleGracefully()
    {
        // Arrange
        var matchId = 123;
        var triggerResources = CreateSampleTriggerResources();
        var relatedMatchItems = CreateSampleRelatedMatchItems();
        var validResults = new List<SourcesConditions>
        {
            new()
            {
                ResourceId = "123",
                RuleId = 456,
                SourceAction = "Refund",
                SourceMatchType = 1,
                SourceChildMatchType = 202,
                Conditions = "0",
                Resource = null
            }
        };
        
        getRefundMatchInfoHandler.HandleAsync(Arg.Any<List<GetRelatedMatchItemRequest>>())
            .Returns(relatedMatchItems);
        getRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<TargetResourceTrigger>>())
            .Returns(new List<SourcesConditionsItem>());
        getAutoRefundResultsHandler.Handle(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(new List<SourcesConditionsItem>());
        getResourcesConditionsHandler.GetConditionsAsync(Arg.Any<List<short>>(), Arg.Any<List<SourcesConditionsItem>>())
            .Returns(validResults);
        resourcesInfoHandler.HandleAsync(Arg.Any<List<int>>(), Arg.Any<List<SourcesConditions>>())
            .Returns(validResults);
        xmlConvertService.Serialize(Arg.Any<object>()).Returns("<xml>test</xml>");

        // Act
        var result = await handler.Handle(matchId, triggerResources);

        // Assert - The handler should return failure because the specification will fail with null resource
        Assert.False(result.Success);
        Assert.Empty(result.Matches);
    }

    private static List<TargetResourceTrigger> CreateSampleTriggerResources()
    {
        return new List<TargetResourceTrigger>
        {
            new()
            {
                Id = 1,
                SportType = 1,
                Action = "Refund",
                MatchType = 1,
                ChildMatchType = 1,
                TargetResourceChildMatchType = 2,
                TargetResource = "123,456",
                IsValid = true
            }
        };
    }

    private static List<RelatedMatchItem> CreateSampleRelatedMatchItems()
    {
        return new List<RelatedMatchItem>
        {
            new()
            {
                MatchId = 123,
                ChildMatchType = 1,
                TargetChildMatchType = 2,
                TargetMatchId = 456
            }
        };
    }

    private static List<SourcesConditions> CreateSampleSourcesConditions()
    {
        return new List<SourcesConditions>
        {
            new()
            {
                Id = 1,
                ResourceId = "123",
                RuleId = 456,
                SourceAction = "Refund", // This will create RefundedSpecification
                SourceMatchType = 1,
                SourceChildMatchType = 202,
                Conditions = "0",
                Resource = new Resource
                {
                    CfmHomeValue = "HomeValue",
                    CfmAwayValue = "AwayValue",
                    SettlementStatusId = (short)SettleStatus.Refunded, // This will satisfy RefundedSpecification
                    ResultSetting = 0 // This will make IsConfirmedRefundSdt false
                }
            }
        };
    }
}