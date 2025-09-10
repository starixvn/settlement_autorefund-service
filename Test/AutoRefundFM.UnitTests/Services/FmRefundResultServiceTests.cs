using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services;

public class FmRefundResultServiceTests
{
    private readonly AutoRefundResultService refundResultService;
    private readonly XmlConvertService xmlConvertService;
    private readonly FmRefundResultService sut;

    public FmRefundResultServiceTests()
    {
        refundResultService = Substitute.For<AutoRefundResultService>();
        xmlConvertService = Substitute.For<XmlConvertService>();
        sut = new FmRefundResultService(refundResultService, xmlConvertService);
    }

    #region AddOrUpdateSessionResult Tests

    [Fact]
    public async Task AddOrUpdateSessionResult_WithValidSessions_CallsInsertOrUpdateResults_WithCorrectParameters()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" },
            new SpecialSession { ResourceId = "R2", HomeValue = "3", AwayValue = "4", OtherInfo = "info2" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(10, sessions, 5, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(10, 5, "<xml/>");
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_WithEmptySessionsList_CallsInsertOrUpdateResults_WithEmptyXml()
    {
        // Arrange
        var sessions = new List<SpecialSession>();
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(10, sessions, 5, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(10, 5, "<xml/>");
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r => !r.ResourceItems.Any()));
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_WithNullErrorMsg_UsesEmptyString()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(10, sessions, 5);

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(10, 5, "<xml/>");
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_WithSingleSession_MapsCorrectly()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(10, sessions, 5, "err");

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r =>
            r.ResourceItems.Count == 1 &&
            r.ResourceItems[0].ResourceId == "R1" &&
            r.ResourceItems[0].HomeValue == "1" &&
            r.ResourceItems[0].AwayValue == "2" &&
            r.ResourceItems[0].OtherInfo == "info" &&
            r.ResourceItems[0].Remark == "err"));
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_WithMultipleSessions_MapsAllCorrectly()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info1" },
            new SpecialSession { ResourceId = "R2", HomeValue = "3", AwayValue = "4", OtherInfo = "info2" },
            new SpecialSession { ResourceId = "R3", HomeValue = "5", AwayValue = "6", OtherInfo = "info3" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(10, sessions, 5, "err");

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r =>
            r.ResourceItems.Count == 3 &&
            r.ResourceItems[0].ResourceId == "R1" &&
            r.ResourceItems[1].ResourceId == "R2" &&
            r.ResourceItems[2].ResourceId == "R3"));
    }

    #endregion

    #region AddOrUpdateFmResult Tests

    [Fact]
    public async Task AddOrUpdateFmResult_WithValidFastMarketResults_CallsInsertOrUpdateResults_WithCorrectParameters()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 },
            new FastMarketResultDetail { BetType = 4, Position = 5, Result = 6 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml2/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, 7, "err2");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(20, 7, "<xml2/>");
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithEmptyFastMarketResultsList_CallsInsertOrUpdateResults_WithEmptyXml()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>();
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, 7, "err2");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(20, 7, "<xml/>");
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r => !r.ResourceItems.Any()));
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithNullErrorMsg_UsesEmptyString()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, 7);

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(20, 7, "<xml/>");
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithSingleFastMarketResult_MapsCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, 7, "err2");

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r =>
            r.ResourceItems.Count == 1 &&
            r.ResourceItems[0].ResourceId == "1" &&
            r.ResourceItems[0].OtherInfo == "2" &&
            r.ResourceItems[0].HomeValue == "3" &&
            r.ResourceItems[0].Remark == "err2"));
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithMultipleFastMarketResults_MapsAllCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 },
            new FastMarketResultDetail { BetType = 4, Position = 5, Result = 6 },
            new FastMarketResultDetail { BetType = 7, Position = 8, Result = 9 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, 7, "err2");

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r =>
            r.ResourceItems.Count == 3 &&
            r.ResourceItems[0].ResourceId == "1" &&
            r.ResourceItems[1].ResourceId == "4" &&
            r.ResourceItems[2].ResourceId == "7"));
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithNullResultValue_MapsCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = null }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, 7, "err2");

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r =>
            r.ResourceItems.Count == 1 &&
            r.ResourceItems[0].ResourceId == "1" &&
            r.ResourceItems[0].OtherInfo == "2" &&
            r.ResourceItems[0].HomeValue == "0" && // null Result should map to 0
            r.ResourceItems[0].Remark == "err2"));
    }

    #endregion

    #region CreateResourceItemsFromSessions Tests

    [Fact]
    public void CreateResourceItemsFromSessions_WithValidSessions_MapsPropertiesCorrectly()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" }
        };

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromSessions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { sessions, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("R1", result[0].ResourceId);
        Assert.Equal("1", result[0].HomeValue);
        Assert.Equal("2", result[0].AwayValue);
        Assert.Equal("info", result[0].OtherInfo);
        Assert.Equal("err", result[0].Remark);
    }

    [Fact]
    public void CreateResourceItemsFromSessions_WithEmptySessionsList_ReturnsEmptyList()
    {
        // Arrange
        var sessions = new List<SpecialSession>();

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromSessions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { sessions, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateResourceItemsFromSessions_WithMultipleSessions_MapsAllCorrectly()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info1" },
            new SpecialSession { ResourceId = "R2", HomeValue = "3", AwayValue = "4", OtherInfo = "info2" }
        };

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromSessions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { sessions, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("R1", result[0].ResourceId);
        Assert.Equal("R2", result[1].ResourceId);
    }

    #endregion

    #region CreateResourceItemsFromBetTypes Tests

    [Fact]
    public void CreateResourceItemsFromBetTypes_WithValidFastMarketResults_MapsPropertiesCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 }
        };

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromBetTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { fastMarketResults, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("1", result[0].ResourceId);
        Assert.Equal("2", result[0].OtherInfo);
        Assert.Equal("3", result[0].HomeValue);
        Assert.Equal("err", result[0].Remark);
    }

    [Fact]
    public void CreateResourceItemsFromBetTypes_WithEmptyFastMarketResultsList_ReturnsEmptyList()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>();

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromBetTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { fastMarketResults, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateResourceItemsFromBetTypes_WithMultipleFastMarketResults_MapsAllCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 },
            new FastMarketResultDetail { BetType = 4, Position = 5, Result = 6 }
        };

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromBetTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { fastMarketResults, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].ResourceId);
        Assert.Equal("4", result[1].ResourceId);
    }

    [Fact]
    public void CreateResourceItemsFromBetTypes_WithNullResultValue_MapsToZero()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = null }
        };

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceItemsFromBetTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { fastMarketResults, "err" }) as List<ResourceItem>;

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("0", result[0].HomeValue); // null Result should map to 0
    }

    #endregion

    #region CreateResourceXml Tests

    [Fact]
    public void CreateResourceXml_WithValidResourceItems_ReturnsSerializedXml()
    {
        // Arrange
        var resourceItems = new List<ResourceItem>
        {
            new ResourceItem { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info", Remark = "err" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceXml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(sut, new object[] { resourceItems }) as string;

        // Assert
        Assert.Equal("<xml/>", result);
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r =>
            r.ResourceItems.Count == 1 &&
            r.ResourceItems[0].ResourceId == "R1"));
    }

    [Fact]
    public void CreateResourceXml_WithEmptyResourceItems_ReturnsSerializedXml()
    {
        // Arrange
        var resourceItems = new List<ResourceItem>();
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        var result = typeof(FmRefundResultService)
            .GetMethod("CreateResourceXml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(sut, new object[] { resourceItems }) as string;

        // Assert
        Assert.Equal("<xml/>", result);
        xmlConvertService.Received(1).Serialize(Arg.Is<Resources<ResourceItem>>(r => !r.ResourceItems.Any()));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task AddOrUpdateSessionResult_WhenRefundResultServiceThrows_PropagatesException()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");
        refundResultService.InsertOrUpdateResults(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
            .Returns(Task.FromException(new InvalidOperationException("Database error")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddOrUpdateSessionResult(10, sessions, 5, "err"));
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WhenRefundResultServiceThrows_PropagatesException()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");
        refundResultService.InsertOrUpdateResults(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
            .Returns(Task.FromException(new InvalidOperationException("Database error")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddOrUpdateFmResult(20, fastMarketResults, 7, "err"));
    }



    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task AddOrUpdateSessionResult_WithZeroMatchId_PassesCorrectly()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(0, sessions, 5, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(0, 5, "<xml/>");
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithZeroMatchId_PassesCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(0, fastMarketResults, 7, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(0, 7, "<xml/>");
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_WithNegativeStatusId_PassesCorrectly()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", HomeValue = "1", AwayValue = "2", OtherInfo = "info" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateSessionResult(10, sessions, -1, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(10, -1, "<xml/>");
    }

    [Fact]
    public async Task AddOrUpdateFmResult_WithNegativeStatusId_PassesCorrectly()
    {
        // Arrange
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 1, Position = 2, Result = 3 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml/>");

        // Act
        await sut.AddOrUpdateFmResult(20, fastMarketResults, -1, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(20, -1, "<xml/>");
    }

    #endregion
}