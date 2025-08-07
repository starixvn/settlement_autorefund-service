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
    public async Task Handle_ByMatch_CallsSerializeAndRepository()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 123,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession
                {
                    ResourceId = "R1",
                    StartMinute = 10,
                    EndMinute = 20,
                    OtherInfo = "info"
                }
            }
        };
        xmlConvertService.Serialize(Arg.Any<WaterBreakResource<List<WaterBreakModel>>>()).Returns("<xml>");

        // Act
        await sut.Handle(match);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Any<WaterBreakResource<List<WaterBreakModel>>>());
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateWaterBreakCriteria>(c =>
            c.MatchId == 123 && c.SpecialSession == "<xml>"));
    }

    [Fact]
    public async Task Handle_ByMatchIdAndSessions_CallsHandleWithConstructedMatch()
    {
        // Arrange
        var sessions = new List<SpecialSession>
        {
            new SpecialSession
            {
                ResourceId = "R2",
                StartMinute = 5,
                EndMinute = 15,
                OtherInfo = "other"
            }
        };
        xmlConvertService.Serialize(Arg.Any<WaterBreakResource<List<WaterBreakModel>>>()).Returns("<xml2>");

        // Act
        await sut.Handle(456, sessions);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Any<WaterBreakResource<List<WaterBreakModel>>>());
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateWaterBreakCriteria>(c =>
            c.MatchId == 456 && c.SpecialSession == "<xml2>"));
    }
}
