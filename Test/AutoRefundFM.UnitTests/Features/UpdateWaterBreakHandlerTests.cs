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

    [Fact]
    public async Task Handle_ShouldSerializeResourcesAndExecuteCriteria()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 42,
            SpecialSessions = new List<SpecialSession>
            {
                new SpecialSession { ResourceId = "R1", StartMinute = 10, EndMinute = 20, OtherInfo = "info1" },
                new SpecialSession { ResourceId = "R2", StartMinute = 30, EndMinute = 40, OtherInfo = "info2" }
            }
        };
        var expectedXml = "<xml>resources</xml>";
        xmlConvertService.Serialize(Arg.Any<object>()).Returns(expectedXml);

        var handler = new UpdateWaterBreakHandler(repository, xmlConvertService);

        // Act
        await handler.Handle(match);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<WaterBreakResource<List<WaterBreakModel>>>(r =>
            r.Item.Count == 2 &&
            r.Item.Any(i => i.ResourceId == "R1" && i.HomeValue == 10 && i.AwayValue == 20 && i.OtherInfo == "info1") &&
            r.Item.Any(i => i.ResourceId == "R2" && i.HomeValue == 30 && i.AwayValue == 40 && i.OtherInfo == "info2")
        ));
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateWaterBreakCriteria>(c =>
            c.MatchId == 42 &&
            c.SpecialSession == expectedXml
        ));
    }

    [Fact]
    public async Task Handle_WithNoSpecialSessions_ShouldSerializeEmptyList()
    {
        // Arrange
        var match = new Match { MatchId = 99, SpecialSessions = new List<SpecialSession>() };
        var expectedXml = "<empty/>";
        xmlConvertService.Serialize(Arg.Any<object>()).Returns(expectedXml);
        var handler = new UpdateWaterBreakHandler(repository, xmlConvertService);

        // Act
        await handler.Handle(match);

        // Assert
        xmlConvertService.Received(1).Serialize(Arg.Is<WaterBreakResource<List<WaterBreakModel>>>(r => r.Item.Count == 0));
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateWaterBreakCriteria>(c =>
            c.MatchId == 99 &&
            c.SpecialSession == expectedXml
        ));
    }
}
