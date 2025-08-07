using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services;

public class FastMarketDataTransformationServiceTests
{
    private readonly XmlConvertService xmlConvertService = Substitute.For<XmlConvertService>();
    private readonly FastMarketDataTransformationService sut;

    public FastMarketDataTransformationServiceTests()
    {
        sut = new FastMarketDataTransformationService(xmlConvertService);
    }

    [Fact]
    public void CreateBetTypesRoot_ReturnsBetTypesRootAndXml()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail
                    {
                        BetType = 1,
                        Position = 2,
                        Result = 3,
                    }
                }
            }
        };
        xmlConvertService.Serialize(Arg.Any<BetTypesRoot>()).Returns("<bettypesxml>");

        // Act
        var (betTypesRoot, betTypesRootXml) = sut.CreateBetTypesRoot(sessions);

        // Assert
        Assert.NotNull(betTypesRoot);
        Assert.Equal("<bettypesxml>", betTypesRootXml);
        xmlConvertService.Received(1).Serialize(Arg.Any<BetTypesRoot>());
    }

    [Fact]
    public void CreateResourceItemsFromResult_ReturnsResourceItemsAndXml()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                FastMarketResultDetails = new List<FastMarketResultDetail>
                {
                    new FastMarketResultDetail
                    {
                        Result = 10,
                        Position = 1,
                        SourceId = "SRC",
                        BetType = 1
                    }
                }
            }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<resourcesxml>");

        // Act
        var (resourceItems, resourceXml) = sut.CreateResourceItemsFromResult(sessions, "errMsg");

        // Assert
        Assert.Single(resourceItems);
        Assert.Equal("1", resourceItems[0].ResourceId);
        Assert.Equal("1", resourceItems[0].OtherInfo);
        Assert.Equal("<resourcesxml>", resourceXml);
        xmlConvertService.Received(1).Serialize(Arg.Any<Resources<ResourceItem>>());
    }
}
