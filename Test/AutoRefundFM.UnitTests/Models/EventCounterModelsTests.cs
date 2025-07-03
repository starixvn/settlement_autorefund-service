using AutoRefundFM.Models;
using System.Xml.Serialization;

namespace AutoRefundFM.UnitTests.Models;

public class EventCounterModelsTests
{

    [Theory]
    [InlineData(1, "01:00-01:59")]
    [InlineData(5, "05:00-05:59")]
    [InlineData(9, "09:00-09:59")]
    [InlineData(10, "10:00-10:59")]
    [InlineData(15, "15:00-15:59")]
    public void PositionName_ShouldReturnCorrectTimeRange(int position, string expected)
    {
        // Arrange
        var counter = new EventCounter { Position = position };

        // Act
        var result = counter.PositionName;

        // Assert
        Assert.Equal(expected, result);
    }


    [Fact]
    public void EventResultItem_WhenCheckedIsTrue_ShouldReturnCachedValue()
    {
        // Arrange
        var counter = new EventCounter
        {
            Result = "123456",
            Checked = true,
            EventResultItem = new EventResultItem
            {
                Goal = "Test",
                Corner = "Test",
                FreeKick = "Test",
                GoalKick = "Test",
                ThrowIn = "Test",
                Penalty = "Test"
            }
        };

        // Act
        var result = counter.EventResultItem;

        // Assert
        Assert.Equal("Test", result.Goal);
        Assert.Equal("Test", result.Corner);
        Assert.Equal("Test", result.FreeKick);
        Assert.Equal("Test", result.GoalKick);
        Assert.Equal("Test", result.ThrowIn);
        Assert.Equal("Test", result.Penalty);
    }


    [Fact]
    public void Deserialize_ShouldCreateValidEventCountersRoot()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <EventCounters>
                    <EventCounter Id=""1"">
                        <Item Position=""1"" Result=""123456""/>
                        <Item Position=""2"" Result=""654321""/>
                    </EventCounter>
                </EventCounters>";

        // Act
        var serializer = new XmlSerializer(typeof(EventCountersRoot));
        EventCountersRoot result;
        using (var reader = new StringReader(xml))
        {
            result = (EventCountersRoot)serializer.Deserialize(reader);
        }

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.EventCounter);
        Assert.Equal("1", result.EventCounter[0].Id);
        Assert.Equal(2, result.EventCounter[0].Items.Count);
        Assert.Equal(1, result.EventCounter[0].Items[0].Position);
        Assert.Equal("123456", result.EventCounter[0].Items[0].Result);
        Assert.Equal(2, result.EventCounter[0].Items[1].Position);
        Assert.Equal("654321", result.EventCounter[0].Items[1].Result);
    }
}