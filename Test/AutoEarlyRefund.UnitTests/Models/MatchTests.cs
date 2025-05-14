using AutoEarlyRefund.Models;
using AutoRefund.Core.Models;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Models;

public class MatchTests
{
    [Fact]
    public void ResourceItems_ShouldMapResultDetailsCorrectly()
    {
        // Arrange
        var resultDetails = new List<ResultDetail>
        {
            new ResultDetail { ResourceId = "1", HomeValue = "Home1", AwayValue = "Away1", Remark = "Remark1", SourceId = "Source1", OtherInfo = "Info1" },
            new ResultDetail { ResourceId = "2", HomeValue = "Home2", AwayValue = "Away2", Remark = "Remark2", SourceId = "Source2", OtherInfo = "Info2" }
        };

        var match = new Match { ResultDetails = resultDetails };

        // Act
        var resourceItems = match.ResourceItems.ToList();

        // Assert
        Assert.Equal(2, resourceItems.Count);
        Assert.Equal("1", resourceItems[0].ResourceId);
        Assert.Equal("Home1", resourceItems[0].HomeValue);
        Assert.Equal("Away1", resourceItems[0].AwayValue);
        Assert.Equal("Remark1", resourceItems[0].Remark);
        Assert.Equal("Source1", resourceItems[0].SourceId);
        Assert.Equal("Info1", resourceItems[0].OtherInfo);

        Assert.Equal("2", resourceItems[1].ResourceId);
        Assert.Equal("Home2", resourceItems[1].HomeValue);
        Assert.Equal("Away2", resourceItems[1].AwayValue);
        Assert.Equal("Remark2", resourceItems[1].Remark);
        Assert.Equal("Source2", resourceItems[1].SourceId);
        Assert.Equal("Info2", resourceItems[1].OtherInfo);
    }

    [Fact]
    public void WithResourcesXml_ShouldSerializeResourcesCorrectly()
    {
        // Arrange
        var resultDetails = new List<ResultDetail>
        {
            new ResultDetail { ResourceId = "1", HomeValue = "Home1", AwayValue = "Away1", Remark = "Remark1", SourceId = "Source1", OtherInfo = "Info1" }
        };

        var match = new Match { ResultDetails = resultDetails };

        var mockXmlConvertService = Substitute.For<XmlConvertService>();
        mockXmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>())
            .Returns("<Resources><ResourceItem ResourceId=\"1\" HomeValue=\"Home1\" AwayValue=\"Away1\" Remark=\"Remark1\" SourceId=\"Source1\" OtherInfo=\"Info1\" /></Resources>");

        // Act
        match.WithResourcesXml(mockXmlConvertService);

        // Assert
        Assert.Equal("<Resources><ResourceItem ResourceId=\"1\" HomeValue=\"Home1\" AwayValue=\"Away1\" Remark=\"Remark1\" SourceId=\"Source1\" OtherInfo=\"Info1\" /></Resources>", match.ResourcesXml);
    }

    [Fact]
    public void Equals_ShouldReturnTrueForSameMatchIdAndChildMatchType()
    {
        // Arrange
        var match1 = new Match { MatchId = 1, ChildMatchType = 2 };
        var match2 = new Match { MatchId = 1, ChildMatchType = 2 };

        // Act
        var result = match1.Equals(match2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentMatchIdOrChildMatchType()
    {
        // Arrange
        var match1 = new Match { MatchId = 1, ChildMatchType = 2 };
        var match2 = new Match { MatchId = 1, ChildMatchType = 3 };
        var match3 = new Match { MatchId = 2, ChildMatchType = 2 };

        // Act
        var result1 = match1.Equals(match2);
        var result2 = match1.Equals(match3);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHashCodeForSameMatchIdAndChildMatchType()
    {
        // Arrange
        var match1 = new Match { MatchId = 1, ChildMatchType = 2 };
        var match2 = new Match { MatchId = 1, ChildMatchType = 2 };

        // Act
        var hashCode1 = match1.GetHashCode();
        var hashCode2 = match2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashCodeForDifferentMatchIdOrChildMatchType()
    {
        // Arrange
        var match1 = new Match { MatchId = 1, ChildMatchType = 2 };
        var match2 = new Match { MatchId = 1, ChildMatchType = 3 };
        var match3 = new Match { MatchId = 2, ChildMatchType = 2 };

        // Act
        var hashCode1 = match1.GetHashCode();
        var hashCode2 = match2.GetHashCode();
        var hashCode3 = match3.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
        Assert.NotEqual(hashCode1, hashCode3);
    }
}
