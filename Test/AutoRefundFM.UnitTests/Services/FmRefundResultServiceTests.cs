using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services;

public class FmRefundResultServiceTests
{
    private readonly AutoRefundResultService refundResultService = Substitute.For<AutoRefundResultService>();
    private readonly XmlConvertService xmlConvertService = Substitute.For<XmlConvertService>();
    private readonly FmRefundResultService sut;

    public FmRefundResultServiceTests()
    {
        sut = new FmRefundResultService(refundResultService, xmlConvertService);
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_ByMatchIdAndSessions_CallsInsertOrUpdateResults()
    {
        // Arrange
        var matchId = 1;
        var sessions = new List<SpecialSession>
        {
            new SpecialSession { ResourceId = "R1", StartMinute = 10, EndMinute = 20, OtherInfo = "info" }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml>");

        // Act
        await sut.AddOrUpdateSessionResult(matchId, sessions, 2, "err");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(matchId, 2, "<xml>");
    }

    [Fact]
    public async Task AddOrUpdateSessionResult_ByMatch_CallsInsertOrUpdateResults()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 2,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession { ResourceId = "R2", StartMinute = 5, EndMinute = 15, OtherInfo = "other" }
            }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml2>");

        // Act
        await sut.AddOrUpdateSessionResult(match, 3, "msg");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(match.MatchId, 3, "<xml2>");
    }

    [Fact]
    public async Task AddOrUpdateBetTypeResult_CallsInsertOrUpdateResults()
    {
        // Arrange
        var matchId = 3;
        var fastMarketResults = new List<FastMarketResultDetail>
        {
            new FastMarketResultDetail { BetType = 7, Position = 1 }
        };
        xmlConvertService.Serialize(Arg.Any<Resources<ResourceItem>>()).Returns("<xml3>");

        // Act
        await sut.AddOrUpdateBetTypeResult(matchId, fastMarketResults, 4, "errMsg");

        // Assert
        await refundResultService.Received(1).InsertOrUpdateResults(matchId, 4, "<xml3>");
    }
}
