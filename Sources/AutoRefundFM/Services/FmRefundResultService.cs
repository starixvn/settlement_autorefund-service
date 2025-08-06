using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;

namespace AutoRefundFM.Services;


public interface IFmRefundResultService
{
    Task AddOrUpdateBetTypeResult(int matchId, List<FastMarketResultDetail> fastMarketResults, int statusId, string errorMsg);
    Task AddOrUpdateSessionResult(int matchId, List<SpecialSession> specialSessions, int statusId);
    Task AddOrUpdateSessionResult(Match match, int statusId);
}

public class FmRefundResultService(AutoRefundResultService refundResultService, XmlConvertService xmlConvertService) : IFmRefundResultService
{
    public async Task AddOrUpdateSessionResult(int matchId, List<SpecialSession> specialSessions, int statusId)
    {
        await AddOrUpdateSessionResult(new Match
        {
            MatchId = matchId,
            SpecialSessions = specialSessions
        }, statusId);
    }

    public async Task AddOrUpdateSessionResult(Match match, int statusId)
    {
        var resourceItems = match.SpecialSessions
            .Select(x => new ResourceItem
            {
                ResourceId = x.ResourceId,
                HomeValue = $"{x.StartMinute}",
                AwayValue = $"{x.EndMinute}",
                OtherInfo = x.OtherInfo,
            }).ToList();

        var resourceXml = CreateResourceXml(resourceItems);
        await refundResultService.InsertOrUpdateResults(match.MatchId, statusId, resourceXml);
    }

    private string CreateResourceXml(List<ResourceItem> resourceItems)
    {
        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };

        var selectedResources = xmlConvertService.Serialize(resourceRoot);

        return selectedResources;
    }

    public async Task AddOrUpdateBetTypeResult(
        int matchId,
        List<FastMarketResultDetail> fastMarketResults,
        int statusId,
        string errorMsg)
    {
        var newResourceItems = fastMarketResults
            .Select(item => new ResourceItem
            {
                ResourceId = item.BetType.ToString(),
                OtherInfo = item.Position.ToString(),
                Remark = errorMsg
            })
            .ToList();

        var resourceXml = CreateResourceXml(newResourceItems);
        await refundResultService.InsertOrUpdateResults(matchId, statusId, resourceXml);
    }
}
