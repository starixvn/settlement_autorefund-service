using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.CheckingMarkedTicket;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.GetSpecialSession;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
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
    ICheckMarkedTicketHandler checkMarkedTicketHandler,
    IMatchPoolService matchPoolService) : SettleOpaqueFacade
{
    public async Task Refund(Match match)
    {
        try
        {
            var betTypeRoot = await getFastMarketResultsHandler.Handle(match);

            if (await ShouldUpdateWaterBreak(match, betTypeRoot))
                return;

            var (betTypeRootXml, resourceItems, resourceXml) = PrepareRefundData(betTypeRoot);

            var validationResult = await ValidateRefundRequest(match, betTypeRootXml);
            if (!validationResult.IsValid)
            {
                await HandleInValidRefundResult(match.MatchId, resourceItems, validationResult.ErrorMessage);
                return;
            }

            if (await HasMarkedTickets(match.MatchId, betTypeRootXml))
            {
                await HandleInValidRefundResult(match.MatchId, resourceItems, "ERROR! Cannot proceed due to marked tickets. Please unmark them and try again.");
                return;
            }

            await ProcessRefund(match, betTypeRootXml, resourceXml);
        }
        catch (Exception ex)
        {
            await ExceptionLog(match, ex);
        }
        finally
        {
            matchPoolService.RemoveMatch(match);
        }
    }

    private async Task<bool> ShouldUpdateWaterBreak(Match match, BetTypesRoot betTypeRoot)
    {
        if (betTypeRoot.BetType.Count == 0)
        {
            await updateWaterBreakHandler.Handle(match);
            return true;
        }
        return false;
    }

    private (string betTypeRootXml, List<ResourceItem> resourceItems, string resourceXml) PrepareRefundData(BetTypesRoot betTypeRoot)
    {
        var betTypeRootXml = xmlConvertService.Serialize(betTypeRoot);
        var resourceItems = CreateResourceItemsFromBetTypes(betTypeRoot);
        var resourceXml = CreateResourceXml(resourceItems);

        return (betTypeRootXml, resourceItems, resourceXml);
    }

    private async Task<(bool IsValid, string ErrorMessage)> ValidateRefundRequest(Match match, string betTypeRootXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
        };

        var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        validRequest.IsAuto = false;

        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        return validationResult.ErrorCode > 0
            ? (false, validationResult.ErrorMessage)
            : (true, string.Empty);
    }

    private async Task<bool> HasMarkedTickets(int matchId, string betTypeRootXml)
    {
        return await checkMarkedTicketHandler.Handle(matchId, betTypeRootXml);
    }

    private async Task ProcessRefund(Match match, string betTypeRootXml, string resourceXml)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypeRootXml,
        };

        await pushFastMarketResultHandler.Handle(match, criteria, resourceXml);
    }

    public async Task ExceptionLog(Match match, Exception exception)
    {
        var logMessage = new LogMessage($"Match: {match} - {exception.Message}")
            .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
            .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
            .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
            .ToString();

        await Fanex.Logging.LogService.ErrorAsync(logMessage);
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
