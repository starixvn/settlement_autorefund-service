using System.Collections.Generic;
using AutoRefundFM.Models;
using Xunit;

namespace AutoRefundFM.UnitTests.Models;

public class SpecialSessionTests
{
    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var session = new SpecialSession { ResourceId = "R1", OtherInfo = "S:1,E:2" };
        // Act
        var str = session.ToString();
        // Assert
        Assert.Equal("R1(S:1,E:2)", str);
    }

    [Fact]
    public void Equals_True_WhenMatchIdAndResourceIdAreEqual()
    {
        // Arrange
        var s1 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var s2 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        // Act & Assert
        Assert.True(s1.Equals(s2));
    }

    [Fact]
    public void Equals_False_WhenMatchIdOrResourceIdDiffer()
    {
        // Arrange
        var s1 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var s2 = new SpecialSession { MatchId = 2, ResourceId = "A" };
        var s3 = new SpecialSession { MatchId = 1, ResourceId = "B" };
        // Act & Assert
        Assert.False(s1.Equals(s2));
        Assert.False(s1.Equals(s3));
    }

    [Fact]
    public void GetHashCode_ConsistentWithEquals()
    {
        // Arrange
        var s1 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var s2 = new SpecialSession { MatchId = 1, ResourceId = "A" };
        var s3 = new SpecialSession { MatchId = 2, ResourceId = "A" };
        // Act & Assert
        Assert.Equal(s1.GetHashCode(), s2.GetHashCode());
        Assert.NotEqual(s1.GetHashCode(), s3.GetHashCode());
    }

    [Fact]
    public void StartMinute_ParsesHomeValue()
    {
        // Arrange
        var session = new SpecialSession { HomeValue = "12" };
        // Act
        var result = session.StartMinute;
        // Assert
        Assert.Equal(12, result);
    }

    [Fact]
    public void EndMinute_ParsesAwayValue()
    {
        // Arrange
        var session = new SpecialSession { AwayValue = "34" };
        // Act
        var result = session.EndMinute;
        // Assert
        Assert.Equal(34, result);
    }

    [Fact]
    public void StartSecond_ParsesOtherInfo()
    {
        // Arrange
        var session = new SpecialSession { OtherInfo = "S:7,E:8" };
        // Act
        var result = session.StartSecond;
        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void EndSecond_ParsesOtherInfo()
    {
        // Arrange
        var session = new SpecialSession { OtherInfo = "S:7,E:8" };
        // Act
        var result = session.EndSecond;
        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void FastMarketResultDetails_And_EventCounters_DefaultToEmptyList()
    {
        // Arrange
        var session = new SpecialSession();
        // Assert
        Assert.NotNull(session.FastMarketResultDetails);
        Assert.Empty(session.FastMarketResultDetails);
        Assert.NotNull(session.EventCounters);
        Assert.Empty(session.EventCounters);
    }
}
