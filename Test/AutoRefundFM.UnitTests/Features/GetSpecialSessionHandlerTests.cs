using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace AutoRefundFM.UnitTests.Features;

public class GetSpecialSessionHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMatchesWithSpecialSessions()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var resultDetails = new List<ResultDetail>
        {
            new ResultDetail
            {
                MatchId = 1,
                ResourceId = "10",
                HomeValue = "12",
                AwayValue = "15",
                OtherInfo = "Start:30,End:45"
            },
            new ResultDetail
            {
                MatchId = 1,
                ResourceId = "11",
                HomeValue = "16",
                AwayValue = "20",
                OtherInfo = "Start:10,End:20"
            },
            new ResultDetail
            {
                MatchId = 2,
                ResourceId = "12",
                HomeValue = "5",
                AwayValue = "10",
                OtherInfo = "Start:5,End:10"
            }
        };
        repository.FetchAsync<ResultDetail>(Arg.Any<GetSpecialSessionCriteria>()).Returns(resultDetails);

        var handler = new GetSpecialSessionHandler(repository);

        // Act
        var matches = await handler.Handle();

        // Assert
        Assert.Equal(2, matches.Count);
        var match1 = matches.FirstOrDefault(m => m.MatchId == 1);
        Assert.NotNull(match1);
        Assert.Equal(2, match1.SpecialSessions.Count);
        Assert.Equal("10", match1.SpecialSessions[0].ResourceId);
        Assert.Equal(12, match1.SpecialSessions[0].StartMinute);
        Assert.Equal(30, match1.SpecialSessions[0].StartSecond);
        Assert.Equal(15, match1.SpecialSessions[0].EndMinute);
        Assert.Equal(45, match1.SpecialSessions[0].EndSecond);

        var match2 = matches.FirstOrDefault(m => m.MatchId == 2);
        Assert.NotNull(match2);
        Assert.Single(match2.SpecialSessions);
        Assert.Equal("12", match2.SpecialSessions[0].ResourceId);
    }
}