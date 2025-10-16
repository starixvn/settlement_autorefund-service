using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class UpdateWaterBreakHandlerTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly XmlConvertService xmlConvertService = Substitute.For<XmlConvertService>();
    private readonly UpdateWaterBreakHandler sut;

    public UpdateWaterBreakHandlerTests()
    {
        sut = new UpdateWaterBreakHandler(repository, xmlConvertService);
    }

    [Fact]
    public async Task Handle_SerializesAndExecutesWithCorrectData()
    {
        // Arrange
        var matchId = 123;
        var statusId = 2;
        var specialSessions = new List<SpecialSession>
        {
            new()
            {
                ResourceId = "R1",
                HomeValue = "10",
                AwayValue = "15",
                OtherInfo = "info1"
            },
            new()
            {
                ResourceId = "R2",
                HomeValue = "20",
                AwayValue = "25",
                OtherInfo = "info2"
            }
        };

        xmlConvertService.Serialize(Arg.Any<WaterBreakResource<List<WaterBreakModel>>>())
            .Returns("<xml/>");

        // Act
        await sut.Handle(matchId, specialSessions, statusId);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<WaterBreakResource<List<WaterBreakModel>>>(r =>
            r.Item.Count == 2 &&
            r.Item[0].ResourceId == "R1" &&
            r.Item[0].HomeValue == 10 &&
            r.Item[0].AwayValue == 15 &&
            r.Item[0].OtherInfo == "info1" &&
            r.Item[0].StatusId == statusId &&
            r.Item[1].ResourceId == "R2" &&
            r.Item[1].HomeValue == 20 &&
            r.Item[1].AwayValue == 25 &&
            r.Item[1].OtherInfo == "info2" &&
            r.Item[1].StatusId == statusId
        ));

        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateWaterBreakCriteria>(c =>
            c.MatchId == matchId &&
            c.SpecialSession == "<xml/>"
        ));
    }

    [Fact]
    public async Task Handle_WithEmptySpecialSessions_SerializesEmptyList()
    {
        // Arrange
        var matchId = 123;
        var statusId = 2;
        var specialSessions = new List<SpecialSession>();

        // Act
        await sut.Handle(matchId, specialSessions, statusId);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<WaterBreakResource<List<WaterBreakModel>>>(r =>
            r.Item.Count == 0
        ));
    }

    [Fact]
    public async Task Handle_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var matchId = 123;
        var statusId = 2;
        var session = new SpecialSession
        {
            ResourceId = "R1",
            HomeValue = "10",
            AwayValue = "15",
            OtherInfo = "test"
        };

        // Act
        await sut.Handle(matchId, new List<SpecialSession> { session }, statusId);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<WaterBreakResource<List<WaterBreakModel>>>(r =>
            r.Item.Count == 1 &&
            r.Item[0].ResourceId == session.ResourceId &&
            r.Item[0].HomeValue == session.StartMinute &&
            r.Item[0].AwayValue == session.EndMinute &&
            r.Item[0].OtherInfo == session.OtherInfo &&
            r.Item[0].StatusId == statusId
        ));
    }
}
