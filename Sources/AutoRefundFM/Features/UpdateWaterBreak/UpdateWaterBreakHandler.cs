using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;

namespace AutoRefundFM.Features.UpdateWaterBreak;

public interface IUpdateWaterBreakHandler
{
    Task Handle(Match match);
}

public class UpdateWaterBreakHandler : IUpdateWaterBreakHandler
{
    private readonly IDynamicRepository repository;
    private readonly XmlConvertService xmlConvertService;

    public UpdateWaterBreakHandler(IDynamicRepository repository, XmlConvertService xmlConvertService)
    {
        this.repository = repository;
        this.xmlConvertService = xmlConvertService;
    }

    public async Task Handle(Match match)
    {
        var resources = new WaterBreakResource<List<WaterBreakModel>>
        {
            Item = [.. match.SpecialSessions.Select(x => new WaterBreakModel
            {
                ResourceId = x.ResourceId,
                HomeValue = x.StartMinute,
                AwayValue = x.EndMinute,
                OtherInfo = x.OtherInfo,
            })]
        };

        var resourceXml = xmlConvertService.Serialize(resources);

        var criteria = new UpdateWaterBreakCriteria
        {
            MatchId = match.MatchId,
            SpecialSession = resourceXml
        };

        await repository.ExecuteAsync(criteria);
    }
}
