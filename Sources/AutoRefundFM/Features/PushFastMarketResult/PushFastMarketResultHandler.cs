using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;
using Microsoft.Extensions.Options;


namespace AutoRefundFM.Features.PushFastMarketResult;

public interface IPushFastMarketResultHandler
{
    Task Handle(Match match);
}

public class PushFastMarketResultHandler : IPushFastMarketResultHandler
{
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly IGetFastMarketResultsHandler getFastMarketResultsHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly SettlementLogService logService;
    private readonly IDynamicRepository repository;
    private readonly AutoRefundResultService refundResultService;
    private readonly IUpdateWaterBreakHandler updateWaterBreakHandler;
    private readonly AppSettingsKey appSettingsKey;

    public PushFastMarketResultHandler(
        ScaleOutSettleService scaleOutSettleService,
        IGetFastMarketResultsHandler getFastMarketResultsHandler,
        XmlConvertService xmlConvertService,
        SettlementLogService logService,
        IDynamicRepository repository,
        IOptions<AppSettingsKey> appSettingsKey,
        AutoRefundResultService refundResultService,
        IUpdateWaterBreakHandler updateWaterBreakHandler
        )
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.getFastMarketResultsHandler = getFastMarketResultsHandler;
        this.xmlConvertService = xmlConvertService;
        this.logService = logService;
        this.repository = repository;
        this.appSettingsKey = appSettingsKey.Value;
        this.refundResultService = refundResultService;
        this.updateWaterBreakHandler = updateWaterBreakHandler;
    }

    public async Task Handle(Match match)
    {
        var betTypeRoot = await getFastMarketResultsHandler.Handle(match);
        if (betTypeRoot.BetType.Count == 0)
        {
            await updateWaterBreakHandler.Handle(match);
            return;
        }

        var betTypeRootXml = xmlConvertService.Serialize(betTypeRoot);
        var resourceXml = CreateResourceItemsFromBetTypes(betTypeRoot);
        var getRefundResultsItems = betTypeRoot.BetType
            .SelectMany(betType => betType.Items.Select(item => new GetRefundResultsItem(
                match.MatchId,
                betType.Id.ToString(),
                11,
                item.Position.ToString())))
            .ToList();

        var results = await refundResultService.GetResults(getRefundResultsItems);

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
            return;
        }

        var updateMatchInfoRequest = criteria.Adapt<UpdateMatchInfoRequest>();
        updateMatchInfoRequest.Mode = Mode.FastMarket;

        await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);

        await WritePreSettlementLog(match, betTypeRootXml);
        await WriteSettlementLog(match, resourceXml);

        ExecuteCalculate(criteria);

        await updateWaterBreakHandler.Handle(match);
        await refundResultService.InsertOrUpdateResults(match.MatchId, 2, resourceXml);
    }

    private async Task WriteSettlementLog(Match match, string resourceXml)
    {
        var criteria = new InsertSettlementLogCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            Mode = Mode.FastMarket,
            SportType = 1, //soccer
            IsAuto = true,
            SelectedResources = resourceXml,
        };

        await logService.WriteSettlementLog(criteria);
    }

    private async Task WritePreSettlementLog(Match match, string betTypesXml)
    {
        var message = $"NAP: Fast Market REFUND for matchId: {match.MatchId} with {betTypesXml}";

        await logService.WriteUserLogSettle(match.MatchId, message);
    }

    private void ExecuteCalculate(PushFastMarketResultCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            repository.Execute(criteria);
        }
    }
    private string CreateResourceItemsFromBetTypes(BetTypesRoot betTypesRoot)
    {
        var resourceItems = betTypesRoot.BetType
              .SelectMany(betType => betType.Items.Select(item => new ResourceItem
              {
                  ResourceId = betType.Id.ToString(),
                  OtherInfo = item.Position.ToString()
              }))
              .ToList();

        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };

        var selectedResources = xmlConvertService.Serialize(resourceRoot);

        return selectedResources;
    }
}
