using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;

namespace AutoRefundFM.Services;


public interface IFmRefundResultService
{
    Task AddOrUpdateFmResult(int matchId, List<FastMarketResultDetail> fastMarketResults, int statusId, string errorMsg = "");
    Task AddOrUpdateSessionResult(int matchId, List<SpecialSession> specialSessions, int statusId, string errorMsg = "");
}

public class FmRefundResultService(AutoRefundResultService refundResultService, XmlConvertService xmlConvertService) : IFmRefundResultService
{
    public async Task AddOrUpdateSessionResult(int matchId, List<SpecialSession> specialSessions, int statusId, string errorMsg = "")
    {
        var resourceItems = CreateResourceItemsFromSessions(specialSessions, errorMsg);
        var resourceXml = CreateResourceXml(resourceItems);

        await refundResultService.InsertOrUpdateResults(matchId, statusId, resourceXml);
    }

    private static List<ResourceItem> CreateResourceItemsFromSessions(List<SpecialSession> specialSessions, string errorMsg = "")
    {
        return [.. specialSessions
            .Select(session => new ResourceItem
            {
                ResourceId = session.ResourceId,
                HomeValue = session.StartMinute.ToString(),
                AwayValue = session.EndMinute.ToString(),
                OtherInfo = session.OtherInfo,
                Remark = errorMsg,
            })];
    }

    private static List<ResourceItem> CreateResourceItemsFromBetTypes(
        List<FastMarketResultDetail> fastMarketResults,
        string errorMsg)
    {
        return [.. fastMarketResults
            .Select(result => new ResourceItem
            {
                ResourceId = result.BetType.ToString(),
                OtherInfo = result.Position.ToString(),
                HomeValue = result.Result.ToString(),
                Remark = errorMsg
            })];
    }

    private string CreateResourceXml(List<ResourceItem> resourceItems)
    {
        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };


        return xmlConvertService.Serialize(resourceRoot);
    }

    public async Task AddOrUpdateFmResult(
        int matchId,
        List<FastMarketResultDetail> fastMarketResults,
        int statusId,
        string errorMsg = "")
    {
        var resourceItems = CreateResourceItemsFromBetTypes(fastMarketResults, errorMsg);
        var resourceXml = CreateResourceXml(resourceItems);

        await refundResultService.InsertOrUpdateResults(matchId, statusId, resourceXml);
    }
}
