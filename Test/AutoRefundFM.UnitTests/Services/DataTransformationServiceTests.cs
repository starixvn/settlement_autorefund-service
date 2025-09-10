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
    public void CreateResourceItemsFromResult_EmptySessions_ReturnsEmptyListAndXml()
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
}
