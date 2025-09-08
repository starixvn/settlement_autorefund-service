using AutoRefundFM.Models;

namespace AutoRefundFM.UnitTests.Models;

public class EventCounterXmlTests
{
    private List<EventCounter> CreateEventCounters(string? goal = "1", string? corner = "2", string? freeKick = "3", string? goalKick = "4", string? throwIn = "5", string? penalty = "6")
    {
        return new List<EventCounter>
        {
            new EventCounter
            {
                Position = 10,
                Result = "0 01010 "
            },
            new EventCounter
            {
                Position = 20,
                Result = "0 10101 "
            }
        };
    }

    [Fact]
    public void GoalResult_CreatesCorrectElement()
    {
        // Arrange
        var counters = CreateEventCounters();

        // Act
        var element = EventCounterElement.GoalResult(counters);

        // Assert
        Assert.Equal(EventCounterResource.Goal, element.Id);
        Assert.Equal(2, element.Items.Count);
        Assert.Equal(10, element.Items[0].Position);
        Assert.Equal(20, element.Items[1].Position);
        Assert.Equal("0", element.Items[0].Result);
        Assert.Equal("1", element.Items[1].Result);
    }

    [Fact]
    public void CornerResult_CreatesCorrectElement()
    {
        // Arrange
        var counters = CreateEventCounters();

        // Act
        var element = EventCounterElement.CornerResult(counters);

        // Assert
        Assert.Equal(EventCounterResource.Corner, element.Id);
        Assert.Equal(2, element.Items.Count);
        Assert.Equal("1", element.Items[0].Result);
        Assert.Equal("0", element.Items[1].Result);
    }

    [Fact]
    public void FreeKickResult_CreatesCorrectElement()
    {
        // Arrange
        var counters = CreateEventCounters();

        // Act
        var element = EventCounterElement.FreeKickResult(counters);

        // Assert
        Assert.Equal(EventCounterResource.FreeKick, element.Id);
        Assert.Equal(2, element.Items.Count);
        Assert.Equal("0", element.Items[0].Result);
        Assert.Equal("1", element.Items[1].Result);
    }

    [Fact]
    public void GoalKickResult_CreatesCorrectElement()
    {
        // Arrange
        var counters = CreateEventCounters();

        // Act
        var element = EventCounterElement.GoalKickResult(counters);

        // Assert
        Assert.Equal(EventCounterResource.GoalKick, element.Id);
        Assert.Equal(2, element.Items.Count);
        Assert.Equal("1", element.Items[0].Result);
        Assert.Equal("0", element.Items[1].Result);
    }

    [Fact]
    public void ThrowInResult_CreatesCorrectElement()
    {
        // Arrange
        var counters = CreateEventCounters(throwIn: "11");

        // Act
        var element = EventCounterElement.ThrowInResult(counters);

        // Assert
        Assert.Equal(EventCounterResource.ThrowIn, element.Id);
        Assert.Equal(2, element.Items.Count);
        Assert.Equal("0", element.Items[0].Result);
        Assert.Equal("1", element.Items[1].Result);
    }

    [Fact]
    public void PenaltyResult_CreatesCorrectElement()
    {
        // Arrange
        var counters = CreateEventCounters();

        // Act
        var element = EventCounterElement.PenaltyResult(counters);

        // Assert
        Assert.Equal(EventCounterResource.Penalty, element.Id);
        Assert.Equal(2, element.Items.Count);
        Assert.Equal("0", element.Items[0].Result);
        Assert.Equal("0", element.Items[1].Result);
    }

    [Fact]
    public void Create_NullEventCounters_ReturnsEmptyItems()
    {
        // Act
        var element = EventCounterElement.Create(null, "TestId", _ => "test");

        // Assert
        Assert.Empty(element.Items);
        Assert.Equal("TestId", element.Id);
    }

    [Fact]
    public void Create_NullEventResultItem_HandlesGracefully()
    {
        // Arrange
        var counters = new List<EventCounter>
        {
            new EventCounter { Position = 1, EventResultItem = null }
        };

        // Act
        var element = EventCounterElement.GoalResult(counters);

        // Assert
        Assert.Single(element.Items);
        Assert.Equal(string.Empty, element.Items[0].Result);
    }

    [Fact]
    public void Create_NullSelector_HandlesGracefully()
    {
        // Arrange
        var counters = CreateEventCounters();

        // Act
        var element = EventCounterElement.Create(counters, "TestId", _ => null);

        // Assert
        Assert.Equal(2, element.Items.Count);
        Assert.All(element.Items, i => Assert.Equal(string.Empty, i.Result));
    }
}
