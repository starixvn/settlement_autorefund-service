using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;

namespace AutoRefundFM.Services;

public interface IFastMarketDataTransformationService
{
    (BetTypesRoot, string) CreateBetTypesRoot(List<SpecialSession> sessions);
    (List<ResourceItem>, string) CreateResourceItemsFromResult(List<SpecialSession> sessions);
}

public class FastMarketDataTransformationService(XmlConvertService xmlConvertService) : IFastMarketDataTransformationService
{
    public (BetTypesRoot, string) CreateBetTypesRoot(List<SpecialSession> sessions)
    {
        var fastMarketResultDetails = sessions.SelectMany(x => x.FastMarketResultDetails).ToList();
        var betTypesRoot = CreateBetTypesRoot(fastMarketResultDetails);
        var betTypesRootXml = xmlConvertService.Serialize(betTypesRoot);

        return (betTypesRoot, betTypesRootXml);
    }

    public (List<ResourceItem>, string) CreateResourceItemsFromResult(List<SpecialSession> sessions)
    {
        var fastMarketResultDetails = sessions.SelectMany(x => x.FastMarketResultDetails).ToList();

        var resourceItems = fastMarketResultDetails
            .Select(x => new ResourceItem
            {
                HomeValue = x.Result.ToString(),
                OtherInfo = x.Position.ToString(),
                SourceId = x.SourceId,
            })
            .ToList();

        var resourceXml = CreateResourceXml(resourceItems);

        return (resourceItems, resourceXml);
    }



    private static BetTypesRoot CreateBetTypesRoot(List<FastMarketResultDetail> results)
    {
        var betTypes = results.GroupBy(g => g.BetType)
            .OrderBy(g => g.Key)
            .Select(g => new BetTypeElement
            {
                Id = g.Key,
                Items = [.. g.Select(x => new ItemElement
                {
                    Position = x.Position,
                    Result = GetResult(x),

                })]
            }).ToList();

        var betTypesRoot = new BetTypesRoot
        {
            BetType = betTypes ?? []
        };

        return betTypesRoot;
    }

    private static string GetResult(FastMarketResultDetail resultDetail)
    {
        return resultDetail.BetType switch
        {
            FastMarketBetType.TimeMachineMinutes or FastMarketBetType.TimeMachineHdp
                => resultDetail.Result?.ToString() ?? string.Empty,

            FastMarketBetType.NextOneMinuteSetPiece
                => resultDetail.ResultItem.Goal
                    ? SetPiece.CornerFreeKickGoalKickAndPenalty.ToString()
                    : resultDetail.ResultItem.Value.ToString(),

            _ => resultDetail.ResultItem.Value.ToString()
        };
    }

    private string CreateResourceXml(List<ResourceItem> resourceItems)
    {
        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };

        return xmlConvertService.Serialize(resourceRoot);
    }
}
