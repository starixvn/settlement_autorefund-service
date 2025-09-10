using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services;

public class DataTransformationServiceTests
{
    private readonly XmlConvertService xmlConvertService = Substitute.For<XmlConvertService>();
    private readonly DataTransformationService sut;

    public DataTransformationServiceTests()
    {
        sut = new DataTransformationService(xmlConvertService);
    }

    #region CreateEventCountersXml Tests

    [Fact]
    public void CreateEventCountersXml_ReturnsSerializedXml()
    {
        // Arrange
        var eventCounters = new List<EventCounter>
        {
            new EventCounter
            {
                Position = 1,
                EventResultItem = new EventResultItem { Goal = "1", Corner = "2", FreeKick = "3", GoalKick = "4", ThrowIn = "5", Penalty = "6" }
            }
        };
        xmlConvertService.Serialize(Arg.Any<EventCountersRoot>()).Returns("<eventcountersxml>");

        // Act
        var xml = sut.CreateEventCountersXml(eventCounters);

        // Assert
        Assert.Equal("<eventcountersxml>", xml);
        xmlConvertService.Received(1).Serialize(Arg.Any<EventCountersRoot>());
    }

    [Fact]
    public void CreateEventCountersXml_WithEmptyList_ReturnsEmptyXml()
    {
        // Arrange
        var eventCounters = new List<EventCounter>();
        xmlConvertService.Serialize(Arg.Any<EventCountersRoot>()).Returns("<empty>");

        // Act
        var xml = sut.CreateEventCountersXml(eventCounters);

        // Assert
        Assert.Equal("<empty>", xml);
        xmlConvertService.Received(1).Serialize(Arg.Is<EventCountersRoot>(x => x.EventCounter.Count == 0));
    }

    [Fact]
    public void CreateEventCountersXml_WithMultipleCounters_SerializesAllEvents()
    {
        // Arrange
        var eventCounters = new List<EventCounter>
        {
            new() { Position = 1, EventResultItem = new EventResultItem { Goal = "1" } },
            new() { Position = 2, EventResultItem = new EventResultItem { Corner = "2" } }
        };
        xmlConvertService.Serialize(Arg.Any<EventCountersRoot>()).Returns("<multipleevents>");

        // Act
        var xml = sut.CreateEventCountersXml(eventCounters);

        // Assert
        Assert.Equal("<multipleevents>", xml);
        xmlConvertService.Received(1).Serialize(Arg.Is<EventCountersRoot>(x => x.EventCounter.Count == 6));
    }

    #endregion

    #region CreateBetTypesRoot Tests

    [Fact]
    public void CreateBetTypesRoot_EmptyList_ReturnsEmptyBetTypesRoot()
    {
        // Arrange
        var details = new List<FastMarketResultDetail>();
        xmlConvertService.Serialize(Arg.Any<BetTypesRoot>()).Returns("<emptyxml>");

        // Act
        var (betTypesRoot, betTypesRootXml) = sut.CreateBetTypesRoot(details);

        // Assert
        Assert.NotNull(betTypesRoot);
        Assert.Empty(betTypesRoot.BetType);
        Assert.Equal("<emptyxml>", betTypesRootXml);
    }

    [Fact]
    public void CreateBetTypesRoot_WithTimeMachineBetType_ReturnsCorrectResult()
    {
        // Arrange
        var details = new List<FastMarketResultDetail>
        {
            new() { BetType = FastMarketBetType.TimeMachineMinutes, Position = 1, Result = 5 }
        };
        xmlConvertService.Serialize(Arg.Any<BetTypesRoot>()).Returns("<timemachine>");

        // Act
        var (betTypesRoot, betTypesRootXml) = sut.CreateBetTypesRoot(details);

        // Assert
        Assert.Single(betTypesRoot.BetType);
        Assert.Equal(FastMarketBetType.TimeMachineMinutes, betTypesRoot.BetType[0].Id);
        Assert.Equal("5", betTypesRoot.BetType[0].Items[0].Result);
        Assert.Equal("<timemachine>", betTypesRootXml);
    }

    [Fact]
    public void CreateBetTypesRoot_WithMultipleBetTypes_GroupsAndOrdersCorrectly()
    {
        // Arrange
        var details = new List<FastMarketResultDetail>
        {
            new() { BetType = FastMarketBetType.NextOneMinute, Position = 1, ResultItem = new ResultItem { Goal = true } },
            new() { BetType = FastMarketBetType.NextOneMinute, Position = 2, ResultItem = new ResultItem { Corner = true } },
            new() { BetType = FastMarketBetType.NextFiveMinutes, Position = 1, ResultItem = new ResultItem { FreeKick = true } }
        };
        xmlConvertService.Serialize(Arg.Any<BetTypesRoot>()).Returns("<multiple>");

        // Act
        var (betTypesRoot, _) = sut.CreateBetTypesRoot(details);

        // Assert
        Assert.Equal(2, betTypesRoot.BetType.Count);
        Assert.Equal(2, betTypesRoot.BetType.First(x => x.Id == FastMarketBetType.NextOneMinute).Items.Count);
        Assert.Single(betTypesRoot.BetType.First(x => x.Id == FastMarketBetType.NextFiveMinutes).Items);
    }

    #endregion

    #region CreateResourcesFromFmResult Tests

    [Fact]
    public void CreateResourcesFromFmResult_EmptySessions_ReturnsEmptyListAndXml()
    {
        // Arrange
        var sessions = new List<FastMarketResultDetail>();
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<emptyresources>");

        // Act
        var (resourceItems, resourceXml) = sut.CreateResourcesFromFmResult(sessions);

        // Assert
        Assert.Empty(resourceItems);
        Assert.Equal("<emptyresources>", resourceXml);
    }

    [Fact]
    public void CreateResourcesFromFmResult_WithTimeMachineResult_MapsCorrectly()
    {
        // Arrange
        var sessions = new List<FastMarketResultDetail>
        {
            new() { BetType = FastMarketBetType.TimeMachineMinutes, Position = 1, Result = 5 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<resources>");

        // Act
        var (resourceItems, _) = sut.CreateResourcesFromFmResult(sessions);

        // Assert
        Assert.Single(resourceItems);
        var item = resourceItems[0];
        Assert.Equal("5", item.HomeValue);
        Assert.Equal("1", item.OtherInfo);
        Assert.Equal(FastMarketBetType.TimeMachineMinutes.ToString(), item.ResourceId);
    }

    [Fact]
    public void CreateResourcesFromFmResult_WithMultipleResults_MapsAllCorrectly()
    {
        // Arrange
        var sessions = new List<FastMarketResultDetail>
        {
            new() { BetType = FastMarketBetType.NextOneMinute, Position = 1, ResultItem = new ResultItem { Goal = true } },
            new() { BetType = FastMarketBetType.NextFiveMinutes, Position = 2, ResultItem = new ResultItem { Corner = true } }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<resources>");

        // Act
        var (resourceItems, _) = sut.CreateResourcesFromFmResult(sessions);

        // Assert
        Assert.Equal(2, resourceItems.Count);
        Assert.Equal("1", resourceItems[0].OtherInfo);
        Assert.Equal("2", resourceItems[1].OtherInfo);
    }

    [Fact]
    public void CreateResourcesFromFmResult_WithCustomErrorMessage_PreservesErrorMessage()
    {
        // Arrange
        var sessions = new List<FastMarketResultDetail>();
        var errorMsg = "Custom error message";
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<resources>");

        // Act
        var (_, resourceXml) = sut.CreateResourcesFromFmResult(sessions, errorMsg);

        // Assert
        Assert.Equal("<resources>", resourceXml);
        xmlConvertService.Received(1).Serialize(Arg.Any<Resources<ResourceItem>>());
    }

    #endregion
}
