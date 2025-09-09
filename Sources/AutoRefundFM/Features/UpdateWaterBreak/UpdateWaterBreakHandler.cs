using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;

namespace AutoRefundFM.Features.UpdateWaterBreak;

public interface IUpdateWaterBreakHandler
{
    Task Handle(Match match);
    Task Handle(int matchId, List<SpecialSession> SpecialSessions);
}

public class UpdateWaterBreakHandler(
    IDynamicRepository repository,
    XmlConvertService xmlConvertService) : IUpdateWaterBreakHandler
{

    public async Task Handle(Match match)
    {
        await Handle(match.MatchId, [match.SpecialSession]);
    }

    public async Task Handle(int matchId, List<SpecialSession> specialSessions)
    {
        var resources = new WaterBreakResource<List<WaterBreakModel>>
        {
            Item = [..specialSessions.Select(x  => new WaterBreakModel
            {
                ResourceId =x.ResourceId,
                HomeValue =x.StartMinute,
                AwayValue = x.EndMinute,
                OtherInfo = x.OtherInfo,
            })]
        };

        var resourceXml = xmlConvertService.Serialize(resources);

        var criteria = new UpdateWaterBreakCriteria
        {
            MatchId = matchId,
            SpecialSession = resourceXml
        };

        await repository.ExecuteAsync(criteria);
    }
}
