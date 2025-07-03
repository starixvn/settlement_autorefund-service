using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;

namespace AutoRefundFM.Services;
public interface SettleOpaqueFacade
{
    Task Refund(Match match);
}


public class SettleOpaqueFacadeImp(
    IGetFastMarketResultsHandler getFastMarketResultsHandler,
    IUpdateWaterBreakHandler updateWaterBreakHandler,
    ScaleOutSettleService scaleOutSettleService,
    XmlConvertService xmlConvertService,
    IPushFastMarketResultHandler pushFastMarketResultHandler,
    AutoRefundResultService refundResultService,
    ICheckMarkedTicketHandler checkMarkedTicketHandler) : SettleOpaqueFacade
{
    public async Task Refund(Match match)
    {
        var betTypeRoot = await getFastMarketResultsHandler.Handle(match);
        if (betTypeRoot.BetType.Count == 0)
        {
            await updateWaterBreakHandler.Handle(match);
            return;
        }

        var betTypeRootXml = xmlConvertService.Serialize(betTypeRoot);
        var resourceItems = CreateResourceItemsFromBetTypes(betTypeRoot);
        var resourceXml = CreateResourceXml(resourceItems);

        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
        };
        var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        validRequest.IsAuto = false;
        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        if (validationResult.ErrorCode > 0)
        {
            await HandleInValidRefundResult(match.MatchId, resourceItems, validationResult.ErrorMessage);
            return;
        }

        var markedTicketResult = await checkMarkedTicketHandler.Handle(match.MatchId, betTypeRootXml);
        if (markedTicketResult)
        {
            await HandleInValidRefundResult(match.MatchId, resourceItems, "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again.");
            return;
        }

        await pushFastMarketResultHandler.Handle(match, criteria, resourceXml);
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

    private List<ResourceItem> CreateResourceItemsFromBetTypes(BetTypesRoot betTypesRoot)
    {
        return [.. betTypesRoot.BetType
            .SelectMany(betType => betType.Items.Select(item => new ResourceItem
            {
                ResourceId = betType.Id.ToString(),
                OtherInfo = item.Position.ToString()
            }))];
    }

    private async Task HandleInValidRefundResult(int matchId, List<ResourceItem> resourceItems, string errorMsg)
    {
        var newResourceItems = resourceItems
            .Select(item => new ResourceItem
            {
                ResourceId = item.ResourceId,
                OtherInfo = item.OtherInfo,
                Remark = errorMsg
            })
            .ToList();

        var resourceXml = CreateResourceXml(newResourceItems);
        await refundResultService.InsertOrUpdateResults(matchId, 3, resourceXml);
    }
}